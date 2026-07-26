using System.Text.Json;
using CoreLTToeic.Domain.Entities;
using CoreLTToeic.Domain.Enums;
using CoreLTToeic.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLTToeic.Infrastructure.Data.Seeders;

public class ToeicTestVariantSeeder
{
    private static readonly IReadOnlyDictionary<int, int> ExpectedPartCounts =
        new Dictionary<int, int>
        {
            [1] = 6,
            [2] = 25,
            [3] = 39,
            [4] = 30,
            [5] = 30,
            [6] = 16,
            [7] = 54
        };

    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ILogger<ToeicTestVariantSeeder> _logger;

    public ToeicTestVariantSeeder(
        IDbContextFactory<AppDbContext> contextFactory,
        ILogger<ToeicTestVariantSeeder> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task SeedAsync(string jsonFilePath)
    {
        if (!File.Exists(jsonFilePath))
        {
            _logger.LogWarning("TOEIC variant seed data file not found: {path}", jsonFilePath);
            return;
        }

        var json = await File.ReadAllTextAsync(jsonFilePath);
        var data = JsonSerializer.Deserialize<ToeicTestVariantSeedJson>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        ValidateConfig(data, jsonFilePath);

        await using var context = await _contextFactory.CreateDbContextAsync();
        var sourceTest = await context.Tests
            .AsNoTracking()
            .AsSplitQuery()
            .Include(test => test.Parts)
            .Include(test => test.QuestionGroups)
                .ThenInclude(group => group.Images)
            .Include(test => test.Questions)
            .FirstOrDefaultAsync(test => test.Title == data!.SourceTestTitle);

        if (sourceTest == null)
        {
            throw new InvalidDataException(
                $"Cannot create TOEIC variants because source test was not found: {data!.SourceTestTitle}.");
        }

        ValidateSourceTest(sourceTest);

        var requestedTitles = data!.Variants
            .Select(variant => variant.Title)
            .ToList();
        var existingTests = await context.Tests
            .Where(test => requestedTitles.Contains(test.Title))
            .Select(test => new
            {
                test.Title,
                Category = test.TestCategory != null ? test.TestCategory.Name : null,
                QuestionCount = test.Questions.Count,
                PartCount = test.Parts.Count,
                UserResultCount = test.UserResults.Count
            })
            .ToListAsync();

        var incomplete = existingTests.FirstOrDefault(test =>
            test.QuestionCount != 200 ||
            test.PartCount != 7);
        if (incomplete != null)
        {
            throw new InvalidDataException(
                $"Refusing to replace existing variant {incomplete.Title}: " +
                $"found {incomplete.QuestionCount} questions, {incomplete.PartCount} Parts, " +
                $"and {incomplete.UserResultCount} user results.");
        }

        var variantsByTitle = data!.Variants.ToDictionary(
            variant => variant.Title,
            StringComparer.OrdinalIgnoreCase);
        var wrongCategory = existingTests.FirstOrDefault(test =>
            !string.Equals(
                test.Category,
                variantsByTitle[test.Title].Category,
                StringComparison.OrdinalIgnoreCase));
        if (wrongCategory != null)
        {
            throw new InvalidDataException(
                $"Existing variant {wrongCategory.Title} belongs to category " +
                $"{wrongCategory.Category ?? "(none)"} instead of " +
                $"{variantsByTitle[wrongCategory.Title].Category}.");
        }

        var existingTitles = existingTests
            .Select(test => test.Title)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var variantsToCreate = data.Variants
            .Where(variant => !existingTitles.Contains(variant.Title))
            .ToList();

        if (variantsToCreate.Count == 0)
        {
            _logger.LogInformation(
                "All {count} TOEIC test variants from {path} already exist, skipping.",
                data.Variants.Count,
                jsonFilePath);
            return;
        }

        await using var transaction = await context.Database.BeginTransactionAsync();
        var categoryNames = variantsToCreate
            .Select(variant => variant.Category)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var categories = await context.TestCategories
            .Where(category => categoryNames.Contains(category.Name))
            .ToDictionaryAsync(category => category.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var categoryName in categoryNames)
        {
            if (categories.ContainsKey(categoryName))
                continue;

            var category = new TestCategory { Name = categoryName };
            context.TestCategories.Add(category);
            categories.Add(categoryName, category);
        }

        foreach (var variant in variantsToCreate)
        {
            var clone = CloneTest(
                sourceTest,
                variant,
                categories[variant.Category]);
            context.Tests.Add(clone);
        }

        await context.SaveChangesAsync();

        var storedVariants = await context.Tests
            .Where(test => requestedTitles.Contains(test.Title))
            .Select(test => new
            {
                test.Title,
                Category = test.TestCategory != null ? test.TestCategory.Name : null,
                QuestionCount = test.Questions.Count,
                PartCount = test.Parts.Count
            })
            .ToListAsync();

        if (storedVariants.Count != data.Variants.Count ||
            storedVariants.Any(test =>
                test.QuestionCount != 200 ||
                test.PartCount != 7 ||
                !string.Equals(
                    test.Category,
                    variantsByTitle[test.Title].Category,
                    StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidDataException(
                "TOEIC variant seed completed with an invalid test, Part, or question count.");
        }

        await transaction.CommitAsync();
        _logger.LogInformation(
            "Created {created} TOEIC test variants from {source}; {total} configured variants now exist.",
            variantsToCreate.Count,
            sourceTest.Title,
            storedVariants.Count);
    }

    private static Test CloneTest(
        Test source,
        ToeicTestVariantJson variant,
        TestCategory category)
    {
        var status = Enum.TryParse<TestStatus>(variant.Status, true, out var parsedStatus)
            ? parsedStatus
            : TestStatus.Active;
        var clone = new Test
        {
            Title = variant.Title,
            Duration = variant.Duration,
            Status = status,
            TotalQuestions = 200,
            ListeningAudio = source.ListeningAudio,
            TestCategory = category
        };

        var partsBySourceId = new Dictionary<long, Part>();
        foreach (var sourcePart in source.Parts.OrderBy(part => part.PartNum))
        {
            var part = new Part
            {
                Content = sourcePart.Content,
                PartNum = sourcePart.PartNum,
                StartTimestamp = sourcePart.StartTimestamp,
                Test = clone
            };
            clone.Parts.Add(part);
            partsBySourceId.Add(sourcePart.Id, part);
        }

        var groupsBySourceId = new Dictionary<long, QuestionGroup>();
        foreach (var sourceGroup in source.QuestionGroups.OrderBy(group => group.Id))
        {
            if (!sourceGroup.PartId.HasValue ||
                !partsBySourceId.TryGetValue(sourceGroup.PartId.Value, out var part))
            {
                throw new InvalidDataException(
                    $"Source question group {sourceGroup.Id} cannot resolve its Part.");
            }

            var group = new QuestionGroup
            {
                Audio = sourceGroup.Audio,
                Content = sourceGroup.Content,
                Name = sourceGroup.Name,
                StartTimestamp = sourceGroup.StartTimestamp,
                Part = part,
                Test = clone
            };
            foreach (var sourceImage in sourceGroup.Images)
            {
                group.Images.Add(new QuestionGroupImage
                {
                    Image = sourceImage.Image,
                    QuestionGroup = group
                });
            }

            clone.QuestionGroups.Add(group);
            groupsBySourceId.Add(sourceGroup.Id, group);
        }

        foreach (var sourceQuestion in source.Questions.OrderBy(question => question.OrderNumber))
        {
            if (!sourceQuestion.PartId.HasValue ||
                !partsBySourceId.TryGetValue(sourceQuestion.PartId.Value, out var part))
            {
                throw new InvalidDataException(
                    $"Source question {sourceQuestion.OrderNumber} cannot resolve its Part.");
            }

            QuestionGroup? group = null;
            if (sourceQuestion.QuestionGroupId.HasValue &&
                !groupsBySourceId.TryGetValue(sourceQuestion.QuestionGroupId.Value, out group))
            {
                throw new InvalidDataException(
                    $"Source question {sourceQuestion.OrderNumber} cannot resolve its group.");
            }

            var question = new Question
            {
                Answer1 = sourceQuestion.Answer1,
                Answer2 = sourceQuestion.Answer2,
                Answer3 = sourceQuestion.Answer3,
                Answer4 = sourceQuestion.Answer4,
                Audio = sourceQuestion.Audio,
                Content = sourceQuestion.Content,
                CorrectAnswer = sourceQuestion.CorrectAnswer,
                Image = sourceQuestion.Image,
                OrderNumber = sourceQuestion.OrderNumber,
                StartTimestamp = sourceQuestion.StartTimestamp,
                Transcript = sourceQuestion.Transcript,
                Explanation = sourceQuestion.Explanation,
                Part = part,
                QuestionGroup = group,
                Test = clone
            };
            clone.Questions.Add(question);
        }

        return clone;
    }

    private static void ValidateConfig(
        ToeicTestVariantSeedJson? data,
        string jsonFilePath)
    {
        if (data == null || data.SchemaVersion != 1)
        {
            throw new InvalidDataException(
                $"TOEIC variant seed is empty or has an unsupported schema: {jsonFilePath}.");
        }

        if (string.IsNullOrWhiteSpace(data.SourceTestTitle))
            throw new InvalidDataException("TOEIC variant sourceTestTitle is required.");

        if (data.Variants.Count == 0)
            throw new InvalidDataException("TOEIC variant seed must contain at least one variant.");

        if (data.Variants.Any(variant =>
                string.IsNullOrWhiteSpace(variant.Title) ||
                string.IsNullOrWhiteSpace(variant.Category) ||
                variant.Duration <= 0))
        {
            throw new InvalidDataException(
                "Every TOEIC variant must contain a title, category, and positive duration.");
        }

        if (data.Variants
                .GroupBy(variant => variant.Title, StringComparer.OrdinalIgnoreCase)
                .Any(group => group.Count() > 1))
        {
            throw new InvalidDataException("TOEIC variant titles must be unique.");
        }
    }

    private static void ValidateSourceTest(Test source)
    {
        if (source.Questions.Count != 200)
        {
            throw new InvalidDataException(
                $"Source test must contain exactly 200 questions; found {source.Questions.Count}.");
        }

        var actualNumbers = source.Questions
            .Select(question => question.OrderNumber)
            .OrderBy(number => number)
            .ToArray();
        if (!actualNumbers.SequenceEqual(Enumerable.Range(1, 200)))
            throw new InvalidDataException("Source question numbers must uniquely cover 1 through 200.");

        if (source.Questions.Any(question =>
                question.CorrectAnswer is not ("A" or "B" or "C" or "D")))
        {
            throw new InvalidDataException("Every source question must have an A-D answer.");
        }

        var partCounts = source.Parts.ToDictionary(
            part => (int)part.PartNum,
            part => source.Questions.Count(question => question.PartId == part.Id));
        if (partCounts.Count != ExpectedPartCounts.Count ||
            ExpectedPartCounts.Any(expected =>
                !partCounts.TryGetValue(expected.Key, out var count) ||
                count != expected.Value))
        {
            throw new InvalidDataException(
                "Source Part counts must be 6/25/39/30/30/16/54.");
        }

        var groupsById = source.QuestionGroups.ToDictionary(group => group.Id);
        foreach (var partNumber in Enumerable.Range(1, 4))
        {
            var part = source.Parts.Single(part => (int)part.PartNum == partNumber);
            var partQuestions = source.Questions
                .Where(question => question.PartId == part.Id)
                .ToList();
            var hasAudio = partQuestions.Any(question => !string.IsNullOrWhiteSpace(question.Audio)) ||
                source.QuestionGroups.Any(group =>
                    group.PartId == part.Id &&
                    !string.IsNullOrWhiteSpace(group.Audio));
            if (!hasAudio)
                throw new InvalidDataException($"Source Listening Part {partNumber} has no audio.");

            var missingTranscript = partQuestions.Any(question =>
                string.IsNullOrWhiteSpace(question.Transcript) &&
                (!question.QuestionGroupId.HasValue ||
                 !groupsById.TryGetValue(question.QuestionGroupId.Value, out var group) ||
                 string.IsNullOrWhiteSpace(group.Content)));
            if (missingTranscript)
            {
                throw new InvalidDataException(
                    $"A source question in Listening Part {partNumber} cannot resolve a transcript.");
            }
        }
    }
}
