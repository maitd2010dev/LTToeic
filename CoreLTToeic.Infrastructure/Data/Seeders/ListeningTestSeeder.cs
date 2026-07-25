using System.Text.Json;
using CoreLTToeic.Domain.Entities;
using CoreLTToeic.Domain.Enums;
using CoreLTToeic.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLTToeic.Infrastructure.Data.Seeders;

public class ListeningTestSeeder
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ILogger<ListeningTestSeeder> _logger;

    public ListeningTestSeeder(
        IDbContextFactory<AppDbContext> contextFactory,
        ILogger<ListeningTestSeeder> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task SeedAsync(string jsonFilePath)
    {
        if (!File.Exists(jsonFilePath))
        {
            _logger.LogWarning("Listening seed data file not found: {path}", jsonFilePath);
            return;
        }

        var json = await File.ReadAllTextAsync(jsonFilePath);
        var data = JsonSerializer.Deserialize<ListeningTestJson>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (data == null || data.SchemaVersion != 1 || data.Parts.Count == 0)
        {
            _logger.LogWarning("Listening seed data is empty or has an unsupported schema: {path}", jsonFilePath);
            return;
        }

        Validate(data);

        await using var context = await _contextFactory.CreateDbContextAsync();
        var existing = await context.Tests
            .Include(t => t.UserResults)
            .FirstOrDefaultAsync(t => t.Title == data.Title);

        if (existing != null)
        {
            var storedQuestionCount = await context.Questions.CountAsync(q => q.TestId == existing.Id);
            if (storedQuestionCount == 100)
            {
                _logger.LogInformation("{title} already contains 100 questions, skipping.", data.Title);
                return;
            }

            if (existing.UserResults.Count > 0)
            {
                _logger.LogWarning(
                    "Cannot refresh {title}: the existing test has user results and only {count} questions.",
                    data.Title,
                    storedQuestionCount);
                return;
            }

            context.Tests.Remove(existing);
            await context.SaveChangesAsync();
        }

        var category = await context.TestCategories.FirstOrDefaultAsync(c => c.Name == data.Category);
        if (category == null)
        {
            category = new TestCategory { Name = data.Category };
            context.TestCategories.Add(category);
        }

        var status = Enum.TryParse<TestStatus>(data.Status, true, out var parsedStatus)
            ? parsedStatus
            : TestStatus.Active;

        var test = new Test
        {
            Title = data.Title,
            Duration = data.Duration,
            Status = status,
            TotalQuestions = 100,
            TestCategory = category,
            ListeningAudio = data.Parts
                .SelectMany(p => p.Questions.Select(q => q.Audio)
                    .Concat(p.Groups.Select(g => g.Audio)))
                .FirstOrDefault(a => !string.IsNullOrWhiteSpace(a))
        };

        foreach (var partData in data.Parts.OrderBy(p => p.PartNum))
        {
            var part = new Part
            {
                PartNum = (ToeicLRPart)partData.PartNum,
                Content = partData.Directions,
                Test = test
            };
            test.Parts.Add(part);

            foreach (var questionData in partData.Questions.OrderBy(q => q.OrderNumber))
                test.Questions.Add(MapQuestion(questionData, test, part, null));

            foreach (var groupData in partData.Groups)
            {
                var group = new QuestionGroup
                {
                    Name = groupData.Name,
                    Audio = groupData.Audio,
                    Content = groupData.Content,
                    Test = test,
                    Part = part
                };

                foreach (var image in groupData.Images.Where(i => !string.IsNullOrWhiteSpace(i)))
                    group.Images.Add(new QuestionGroupImage { Image = image });

                foreach (var questionData in groupData.Questions.OrderBy(q => q.OrderNumber))
                {
                    var question = MapQuestion(questionData, test, part, group);
                    group.Questions.Add(question);
                    test.Questions.Add(question);
                }

                test.QuestionGroups.Add(group);
            }
        }

        context.Tests.Add(test);
        await context.SaveChangesAsync();

        _logger.LogInformation(
            "Seeded {title}: {parts} parts, {groups} groups and {questions} questions.",
            test.Title,
            test.Parts.Count,
            test.QuestionGroups.Count,
            test.TotalQuestions);
    }

    private static Question MapQuestion(
        ListeningQuestionJson data,
        Test test,
        Part part,
        QuestionGroup? group)
    {
        return new Question
        {
            OrderNumber = data.OrderNumber,
            Content = data.Content,
            Answer1 = data.Answer1,
            Answer2 = data.Answer2,
            Answer3 = data.Answer3,
            Answer4 = data.Answer4,
            CorrectAnswer = data.CorrectAnswer,
            Image = data.Image,
            Audio = data.Audio,
            Transcript = data.Transcript,
            Test = test,
            Part = part,
            QuestionGroup = group
        };
    }

    private static void Validate(ListeningTestJson data)
    {
        var questions = data.Parts
            .SelectMany(p => p.Questions.Concat(p.Groups.SelectMany(g => g.Questions)))
            .ToList();

        if (questions.Count != 100)
            throw new InvalidDataException($"Listening seed must contain 100 questions, found {questions.Count}.");

        var expectedNumbers = Enumerable.Range(1, 100).ToArray();
        var actualNumbers = questions.Select(q => q.OrderNumber).OrderBy(n => n).ToArray();
        if (!actualNumbers.SequenceEqual(expectedNumbers))
            throw new InvalidDataException("Listening seed question numbers must be unique and cover 1 through 100.");

        if (questions.Any(q => q.CorrectAnswer is not ("A" or "B" or "C" or "D")))
            throw new InvalidDataException("Every listening question must have a valid correct answer.");

        var partCounts = data.Parts.ToDictionary(
            p => p.PartNum,
            p => p.Questions.Count + p.Groups.Sum(g => g.Questions.Count));
        var expectedPartCounts = new Dictionary<int, int>
        {
            [1] = 6,
            [2] = 25,
            [3] = 39,
            [4] = 30
        };

        if (expectedPartCounts.Any(expected =>
                !partCounts.TryGetValue(expected.Key, out var count) || count != expected.Value))
        {
            throw new InvalidDataException("Listening seed part counts must be 6, 25, 39 and 30.");
        }
    }
}
