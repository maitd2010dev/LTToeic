using System.Text.Json;
using CoreLTToeic.Domain.Entities;
using CoreLTToeic.Domain.Enums;
using CoreLTToeic.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLTToeic.Infrastructure.Data.Seeders;

public class ReadingTestSeeder
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ILogger<ReadingTestSeeder> _logger;

    public ReadingTestSeeder(
        IDbContextFactory<AppDbContext> contextFactory,
        ILogger<ReadingTestSeeder> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task SeedAsync(string jsonFilePath)
    {
        if (!File.Exists(jsonFilePath))
        {
            _logger.LogWarning("Reading seed data file not found: {path}", jsonFilePath);
            return;
        }

        var json = await File.ReadAllTextAsync(jsonFilePath);
        var data = JsonSerializer.Deserialize<ReadingTestJson>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (data == null || data.SchemaVersion != 1 || data.Parts.Count == 0)
        {
            _logger.LogWarning(
                "Reading seed data is empty or has an unsupported schema: {path}",
                jsonFilePath);
            return;
        }

        Validate(data);

        await using var context = await _contextFactory.CreateDbContextAsync();
        var test = await context.Tests
            .Include(item => item.Parts)
            .FirstOrDefaultAsync(item => item.Title == data.TargetTestTitle);

        if (test == null)
        {
            _logger.LogWarning(
                "Cannot append Reading data because target test was not found: {title}",
                data.TargetTestTitle);
            return;
        }

        var listeningQuestionCount = await context.Questions.CountAsync(question =>
            question.TestId == test.Id &&
            question.Part != null &&
            question.Part.PartNum >= ToeicLRPart.Part1 &&
            question.Part.PartNum <= ToeicLRPart.Part4);
        var readingQuestionCount = await context.Questions.CountAsync(question =>
            question.TestId == test.Id &&
            question.Part != null &&
            question.Part.PartNum >= ToeicLRPart.Part5 &&
            question.Part.PartNum <= ToeicLRPart.Part7);

        if (listeningQuestionCount != 100)
        {
            throw new InvalidDataException(
                $"Target test must contain exactly 100 Listening questions before Reading is appended; found {listeningQuestionCount}.");
        }

        if (readingQuestionCount == 100)
        {
            test.Duration = Math.Max(test.Duration, data.Duration);
            test.TotalQuestions = 200;
            await context.SaveChangesAsync();
            _logger.LogInformation(
                "{title} already contains 100 Reading questions, skipping.",
                test.Title);
            return;
        }

        if (readingQuestionCount > 0 ||
            test.Parts.Any(part => part.PartNum is >= ToeicLRPart.Part5 and <= ToeicLRPart.Part7))
        {
            throw new InvalidDataException(
                $"Refusing to replace partial Reading data in {test.Title}; found {readingQuestionCount} Reading questions.");
        }

        if (!string.IsNullOrWhiteSpace(data.Category))
        {
            var category = await context.TestCategories
                .FirstOrDefaultAsync(item => item.Name == data.Category);
            if (category == null)
            {
                category = new TestCategory { Name = data.Category };
                context.TestCategories.Add(category);
            }

            test.TestCategory = category;
        }

        foreach (var partData in data.Parts.OrderBy(part => part.PartNum))
        {
            var part = new Part
            {
                PartNum = (ToeicLRPart)partData.PartNum,
                Content = partData.Directions,
                Test = test
            };
            test.Parts.Add(part);

            foreach (var questionData in partData.Questions.OrderBy(question => question.OrderNumber))
                test.Questions.Add(MapQuestion(questionData, test, part, null));

            foreach (var groupData in partData.Groups)
            {
                var group = new QuestionGroup
                {
                    Name = groupData.Name,
                    Content = groupData.Content,
                    Audio = groupData.Audio,
                    Test = test,
                    Part = part
                };

                foreach (var image in groupData.Images.Where(image => !string.IsNullOrWhiteSpace(image)))
                    group.Images.Add(new QuestionGroupImage { Image = image });

                foreach (var questionData in groupData.Questions.OrderBy(question => question.OrderNumber))
                {
                    var question = MapQuestion(questionData, test, part, group);
                    group.Questions.Add(question);
                    test.Questions.Add(question);
                }

                test.QuestionGroups.Add(group);
            }
        }

        test.Duration = Math.Max(test.Duration, data.Duration);
        test.TotalQuestions = 200;
        await context.SaveChangesAsync();

        var storedQuestionCount = await context.Questions.CountAsync(
            question => question.TestId == test.Id);
        if (storedQuestionCount != 200)
        {
            throw new InvalidDataException(
                $"Reading seed completed with an invalid total: expected 200 questions, found {storedQuestionCount}.");
        }

        _logger.LogInformation(
            "Appended Reading data to {title}: {parts} parts, {groups} groups, total {questions} questions.",
            test.Title,
            data.Parts.Count,
            data.Parts.Sum(part => part.Groups.Count),
            storedQuestionCount);
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

    private static void Validate(ReadingTestJson data)
    {
        if (string.IsNullOrWhiteSpace(data.TargetTestTitle))
            throw new InvalidDataException("Reading seed targetTestTitle is required.");

        var questions = data.Parts
            .SelectMany(part => part.Questions.Concat(
                part.Groups.SelectMany(group => group.Questions)))
            .ToList();

        if (questions.Count != 100)
            throw new InvalidDataException(
                $"Reading seed must contain 100 questions, found {questions.Count}.");

        var expectedNumbers = Enumerable.Range(101, 100).ToArray();
        var actualNumbers = questions
            .Select(question => question.OrderNumber)
            .OrderBy(number => number)
            .ToArray();
        if (!actualNumbers.SequenceEqual(expectedNumbers))
        {
            throw new InvalidDataException(
                "Reading seed question numbers must be unique and cover 101 through 200.");
        }

        if (questions.Any(question =>
                question.CorrectAnswer is not ("A" or "B" or "C" or "D")))
        {
            throw new InvalidDataException(
                "Every Reading question must have a valid correct answer.");
        }

        if (questions.Any(question =>
                string.IsNullOrWhiteSpace(question.Content) ||
                string.IsNullOrWhiteSpace(question.Answer1) ||
                string.IsNullOrWhiteSpace(question.Answer2) ||
                string.IsNullOrWhiteSpace(question.Answer3) ||
                string.IsNullOrWhiteSpace(question.Answer4)))
        {
            throw new InvalidDataException(
                "Every Reading question must contain content and four answer choices.");
        }

        var partCounts = data.Parts.ToDictionary(
            part => part.PartNum,
            part => part.Questions.Count +
                    part.Groups.Sum(group => group.Questions.Count));
        var expectedPartCounts = new Dictionary<int, int>
        {
            [5] = 30,
            [6] = 16,
            [7] = 54
        };

        if (partCounts.Count != expectedPartCounts.Count ||
            expectedPartCounts.Any(expected =>
                !partCounts.TryGetValue(expected.Key, out var count) ||
                count != expected.Value))
        {
            throw new InvalidDataException(
                "Reading seed part counts must be 30, 16 and 54.");
        }

        if (data.Parts
            .Where(part => part.PartNum is 6 or 7)
            .SelectMany(part => part.Groups)
            .Any(group => string.IsNullOrWhiteSpace(group.Content)))
        {
            throw new InvalidDataException(
                "Every Part 6/7 group must contain its reading passage.");
        }
    }
}
