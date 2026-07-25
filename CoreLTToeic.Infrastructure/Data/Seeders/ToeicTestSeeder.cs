using System.Text.Json;
using System.Text.RegularExpressions;
using CoreLTToeic.Domain.Entities;
using CoreLTToeic.Domain.Enums;
using CoreLTToeic.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLTToeic.Infrastructure.Data.Seeders;

public class ToeicTestSeeder
{
    private const string Study4TestTitle = "TOEIC LR Collection 1 Test 1";
    private const string LegacyTestTitle = "TOEIC Practice Test 1";

    private static readonly Dictionary<long, int> TypeToPartNum = new()
    {
        { 1636615697542L, 1 },
        { 1636615720709L, 2 },
        { 1636615725762L, 3 },
        { 1636615729794L, 4 },
        { 1636615733972L, 5 },
        { 1636615742506L, 6 },
        { 1636615746924L, 7 },
    };

    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ILogger<ToeicTestSeeder> _logger;

    public ToeicTestSeeder(IDbContextFactory<AppDbContext> contextFactory, ILogger<ToeicTestSeeder> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task SeedAsync(string jsonFilePath)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var category2025 = await EnsureCategoryAsync(context, "2025");
        await EnsureCategoryAsync(context, "2026");
        await context.SaveChangesAsync();

        var existingTest = await context.Tests
            .FirstOrDefaultAsync(t => t.Title == Study4TestTitle || t.Title == LegacyTestTitle);
        if (existingTest != null)
        {
            // Nâng cấp dữ liệu seed cũ thay vì tạo trùng một bộ 200 câu hỏi.
            existingTest.Title = Study4TestTitle;
            existingTest.TestCategoryId = category2025.Id;
            existingTest.Duration = 120;
            await context.SaveChangesAsync();

            var existingQuestionCount = await context.Questions.CountAsync(q => q.TestId == existingTest.Id);
            if (existingQuestionCount > 0)
            {
                existingTest.TotalQuestions = existingQuestionCount;
                await context.SaveChangesAsync();
                _logger.LogInformation(
                    "{title} already seeded with {count} questions; category and metadata were updated.",
                    Study4TestTitle,
                    existingQuestionCount);
                return;
            }
        }

        if (!File.Exists(jsonFilePath))
        {
            _logger.LogWarning("Seed data file not found: {path}", jsonFilePath);
            return;
        }

        var json = await File.ReadAllTextAsync(jsonFilePath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var cards = JsonSerializer.Deserialize<List<ToeicCardJson>>(json, options);
        if (cards == null || cards.Count == 0) return;

        var test = existingTest ?? new Test();
        test.Title = Study4TestTitle;
        test.Duration = 120;
        test.Status = TestStatus.Active;
        test.TotalQuestions = 0;
        test.TestCategory = category2025;

        var parts = Enumerable.Range(1, 7)
            .ToDictionary(n => n, n => new Part { PartNum = (ToeicLRPart)n, Test = test });
        test.Parts = parts.Values.ToList();

        int orderNum = 1;

        foreach (var card in cards.OrderBy(c => c.OrderIndex))
        {
            if (!TypeToPartNum.TryGetValue(card.Type, out int partNum)) continue;
            var part = parts[partNum];

            if (card.HasChild == 1)
            {
                var group = MapToGroup(card, part, test, ref orderNum);
                test.QuestionGroups.Add(group);
            }
            else
            {
                var question = MapToQuestion(card, part, test, null, orderNum++);
                test.Questions.Add(question);
            }
        }

        test.TotalQuestions = orderNum - 1;
        if (existingTest == null)
            context.Tests.Add(test);
        await context.SaveChangesAsync();
        _logger.LogInformation("Seeded {title} in category 2025 with {count} questions.", Study4TestTitle, test.TotalQuestions);
    }

    private static async Task<TestCategory> EnsureCategoryAsync(AppDbContext context, string name)
    {
        var category = await context.TestCategories.FirstOrDefaultAsync(c => c.Name == name);
        if (category != null) return category;

        category = new TestCategory { Name = name };
        context.TestCategories.Add(category);
        return category;
    }

    private static QuestionGroup MapToGroup(ToeicCardJson card, Part part, Test test, ref int orderNum)
    {
        // Part 3/4: question.Text rỗng, transcript ở answer.Hint
        // Part 6/7: passage HTML ở question.Text, answer.Hint rỗng
        var group = new QuestionGroup
        {
            Audio = Null(card.Question.Sound),
            Content = Null(card.Question.Text) ?? Null(card.Answer.Hint),
            Name = null,
            Part = part,
            Test = test
        };

        var resolvedImg = ResolveImage(card.Question.Image);
        if (!string.IsNullOrEmpty(resolvedImg))
            group.Images.Add(new QuestionGroupImage { Image = resolvedImg });

        foreach (var child in card.ChildCards.OrderBy(c => c.OrderIndex))
        {
            group.Questions.Add(MapToQuestion(child, part, test, group, orderNum++));
        }

        return group;
    }

    private static Question MapToQuestion(ToeicCardJson card, Part part, Test test, QuestionGroup? group, int orderNum)
    {
        var (a1, a2, a3, a4, correct) = ParseAnswers(card.Answer);
        return new Question
        {
            Content = Null(card.Question.Text),
            Image = ResolveImage(card.Question.Image),
            Audio = Null(card.Question.Sound),
            Transcript = Null(card.Answer.Hint),
            Answer1 = a1,
            Answer2 = a2,
            Answer3 = a3,
            Answer4 = a4,
            CorrectAnswer = correct,
            OrderNumber = orderNum,
            Part = part,
            Test = test,
            QuestionGroup = group
        };
    }

    private static (string? a1, string? a2, string? a3, string? a4, string? correct) ParseAnswers(ToeicAnswerJson answer)
    {
        if (answer.Texts.Count == 0 && answer.Choices.Count == 0)
            return (null, null, null, null, null);

        var all = answer.Texts.Concat(answer.Choices)
            .OrderBy(ExtractLabel)
            .ToList();

        var correctLabel = answer.Texts.Count > 0 ? ExtractLabel(answer.Texts[0]) : null;

        return (
            all.Count > 0 ? ExtractText(all[0]) : null,
            all.Count > 1 ? ExtractText(all[1]) : null,
            all.Count > 2 ? ExtractText(all[2]) : null,
            all.Count > 3 ? ExtractText(all[3]) : null,
            correctLabel
        );
    }

    private static string ExtractLabel(string s)
    {
        var m = Regex.Match(s, @"\(([A-D])\)");
        return m.Success ? m.Groups[1].Value : s;
    }

    private static string ExtractText(string s)
    {
        // "(A) they" or "(A)they" → "they"; "(A)" → "(A)"
        var m = Regex.Match(s, @"\([A-D]\)\s*(.+)");
        return m.Success ? m.Groups[1].Value.Trim() : s;
    }

    private static string? Null(string s) => string.IsNullOrWhiteSpace(s) ? null : s;

    private static string? ResolveImage(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        if (s.StartsWith("http://") || s.StartsWith("https://")) return s;
        return "https://storage.googleapis.com/" + s.TrimStart('/');
    }
}
