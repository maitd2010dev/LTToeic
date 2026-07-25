using AutoMapper;
using CoreLTToeic.Application.Interfaces.IRepository;
using CoreLTToeic.Application.Interfaces.IService;
using CoreLTToeic.Application.Models.EditModels;
using CoreLTToeic.Application.Models.ViewModels;
using CoreLTToeic.Domain.Entities;
using CoreLTToeic.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CoreLTToeic.Application.Business;

public class UserResultService : IUserResultService
{
    private readonly IUserResultRepository _userResultRepo;
    private readonly IQuestionRepository _questionRepo;
    private readonly IPartRepository _partRepo;
    private readonly IQuestionGroupRepository _groupRepo;
    private readonly IScoreConversionRepository _scoreRepo;
    private readonly IMapper _mapper;

    public UserResultService(
        IUserResultRepository userResultRepo,
        IQuestionRepository questionRepo,
        IPartRepository partRepo,
        IQuestionGroupRepository groupRepo,
        IScoreConversionRepository scoreRepo,
        IMapper mapper)
    {
        _userResultRepo = userResultRepo;
        _questionRepo = questionRepo;
        _partRepo = partRepo;
        _groupRepo = groupRepo;
        _scoreRepo = scoreRepo;
        _mapper = mapper;
    }

    public async Task<long> SubmitTestAsync(UserResultEditModel model, int completionTimeSeconds)
    {
        try
        {
            // Build partId → partNum mapping
            var parts = await _partRepo.GetByTestIdAsync(model.TestId);
            var partNumById = parts.ToDictionary(p => p.Id, p => p.PartNum);

            // Build groupId → partNum mapping (for questions belonging to groups)
            var groups = await _groupRepo.GetByTestIdAsync(model.TestId);
            var groupPartMap = groups
                .Where(g => g.PartId.HasValue && partNumById.ContainsKey(g.PartId.Value))
                .ToDictionary(g => g.Id, g => partNumById[g.PartId!.Value]);

            // Build questionId → partNum & correctAnswer from all test questions
            var allQuestions = await _questionRepo.GetByTestIdAsync(model.TestId);
            var questionPartMap = new Dictionary<long, ToeicLRPart>();
            var correctAnswerMap = new Dictionary<long, string?>();

            foreach (var q in allQuestions)
            {
                correctAnswerMap[q.Id] = q.CorrectAnswer;

                if (q.PartId.HasValue && partNumById.TryGetValue(q.PartId.Value, out var pn))
                    questionPartMap[q.Id] = pn;
                else if (q.QuestionGroupId.HasValue && groupPartMap.TryGetValue(q.QuestionGroupId.Value, out var gp))
                    questionPartMap[q.Id] = gp;
            }

            int correct = 0, incorrect = 0, skipped = 0;
            int listeningCorrects = 0, readingCorrects = 0;
            int totalListening = 0, totalReading = 0;
            var userAnswers = new List<UserAnswer>();

            // Only score questions that belong to this test, and never count a
            // QuestionId more than once even if the client submits duplicates.
            var submittedAnswers = model.Answers
                .Where(answer => correctAnswerMap.ContainsKey(answer.QuestionId))
                .GroupBy(answer => answer.QuestionId)
                .Select(group => group.Last())
                .ToList();

            foreach (var answer in submittedAnswers)
            {
                correctAnswerMap.TryGetValue(answer.QuestionId, out var correctAns);
                bool isSkipped = string.IsNullOrWhiteSpace(answer.SelectedAnswer);
                bool isCorrect = !isSkipped && answer.SelectedAnswer == correctAns;

                if (isSkipped) skipped++;
                else if (isCorrect) correct++;
                else incorrect++;

                if (questionPartMap.TryGetValue(answer.QuestionId, out var partNum))
                {
                    bool isListening = partNum <= ToeicLRPart.Part4;
                    if (isListening) { totalListening++; if (isCorrect) listeningCorrects++; }
                    else { totalReading++; if (isCorrect) readingCorrects++; }
                }

                userAnswers.Add(new UserAnswer
                {
                    QuestionId = answer.QuestionId,
                    SelectedAnswer = answer.SelectedAnswer,
                    IsCorrect = isCorrect
                });
            }

            // Chuẩn hóa về thang 0-100 trước khi lookup bảng quy đổi
            int listeningKey = totalListening > 0
                ? Math.Min((int)Math.Round(listeningCorrects * 100.0 / totalListening), 100) : 0;
            int readingKey = totalReading > 0
                ? Math.Min((int)Math.Round(readingCorrects * 100.0 / totalReading), 100) : 0;

            // A blank section and a section absent from this test both score 0.
            int listeningScore = totalListening > 0 && listeningCorrects > 0
                ? await _scoreRepo.GetListeningScoreAsync(listeningKey)
                : 0;
            int readingScore = totalReading > 0 && readingCorrects > 0
                ? await _scoreRepo.GetReadingScoreAsync(readingKey)
                : 0;

            float accuracy = submittedAnswers.Count > 0
                ? (float)correct / submittedAnswers.Count * 100 : 0;

            var result = new UserResult
            {
                TestId = model.TestId,
                UserId = model.UserId,
                TestMode = model.TestMode,
                CorrectAnswers = correct,
                IncorrectAnswers = incorrect,
                SkippedAnswers = skipped,
                ListeningCorrects = listeningCorrects,
                ReadingCorrects = readingCorrects,
                TotalListeningQuestions = totalListening,
                TotalReadingQuestions = totalReading,
                ListeningScore = listeningScore,
                ReadingScore = readingScore,
                TotalScore = listeningScore + readingScore,
                CompletionTime = completionTimeSeconds,
                Accuracy = accuracy,
                CompletedAt = DateTime.Now,
                AttemptStatus = AttemptStatus.Completed,
                UserAnswers = userAnswers
            };

            _userResultRepo.Add(result);
            await _userResultRepo.SaveChangesAsync();

            return result.Id;
        }
        catch (Exception ex)
        {
            throw new ApplicationException("An error occurred while submitting the test result.", ex);
        }
    }

    public async Task<UserResultViewModel?> GetResultAsync(long resultId)
    {
        var result = await _userResultRepo.GetByIdWithAnswersAsync(resultId);
        return result == null ? null : _mapper.Map<UserResultViewModel>(result);
    }

    public async Task<IEnumerable<UserResultViewModel>> GetByUserIdAsync(string userId)
    {
        var results = await _userResultRepo.GetByUserIdAsync(userId);
        return _mapper.Map<IEnumerable<UserResultViewModel>>(results);
    }
}
