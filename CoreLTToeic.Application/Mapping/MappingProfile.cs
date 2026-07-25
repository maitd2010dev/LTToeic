using AutoMapper;
using CoreLTToeic.Application.Models.EditModels;
using CoreLTToeic.Application.Models.ViewModels;
using CoreLTToeic.Domain.Entities;
using CoreLTToeic.Domain.Enums;
using System.Linq;

namespace CoreLTToeic.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Test, TestViewModel>()
                .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.TestCategory != null ? s.TestCategory.Name : null))
                .ForMember(d => d.AttemptCount, o => o.MapFrom(s => s.UserResults.Count));
            CreateMap<TestEditModel, Test>();

            CreateMap<Question, QuestionViewModel>();
            CreateMap<QuestionEditModel, Question>();

            CreateMap<QuestionGroup, QuestionGroupViewModel>()
                .ForMember(d => d.Images, o => o.MapFrom(s => s.Images.Select(i => i.Image).ToList()))
                .ForMember(d => d.Questions, o => o.MapFrom(s => s.Questions))
                .ForMember(d => d.PartNum, o => o.MapFrom(s => s.Part != null ? (ToeicLRPart?)s.Part.PartNum : null));
            CreateMap<QuestionGroupEditModel, QuestionGroup>()
                .ForMember(d => d.Images, o => o.Ignore())
                .ForMember(d => d.Questions, o => o.Ignore());

            CreateMap<Part, PartViewModel>()
                .ForMember(d => d.QuestionCount, o => o.MapFrom(s => s.Questions.Count))
                .ForMember(d => d.QuestionGroupCount, o => o.MapFrom(s => s.QuestionGroups.Count));
            CreateMap<PartEditModel, Part>();

            CreateMap<TestCategory, TestCategoryViewModel>();

            CreateMap<UserAnswer, UserAnswerViewModel>()
                .ForMember(d => d.CorrectAnswer,     o => o.MapFrom(s => s.Question != null ? s.Question.CorrectAnswer : null))
                .ForMember(d => d.QuestionContent,   o => o.MapFrom(s => s.Question != null ? s.Question.Content : null))
                .ForMember(d => d.Answer1,            o => o.MapFrom(s => s.Question != null ? s.Question.Answer1 : null))
                .ForMember(d => d.Answer2,            o => o.MapFrom(s => s.Question != null ? s.Question.Answer2 : null))
                .ForMember(d => d.Answer3,            o => o.MapFrom(s => s.Question != null ? s.Question.Answer3 : null))
                .ForMember(d => d.Answer4,            o => o.MapFrom(s => s.Question != null ? s.Question.Answer4 : null))
                .ForMember(d => d.Audio,              o => o.MapFrom(s => s.Question != null ? s.Question.Audio : null))
                .ForMember(d => d.Image,              o => o.MapFrom(s => s.Question != null ? s.Question.Image : null))
                .ForMember(d => d.Transcript,         o => o.MapFrom(s => s.Question != null ? s.Question.Transcript : null))
                .ForMember(d => d.Explanation,        o => o.MapFrom(s => s.Question != null ? s.Question.Explanation : null))
                .ForMember(d => d.OrderNumber,        o => o.MapFrom(s => s.Question != null ? s.Question.OrderNumber : 0))
                .ForMember(d => d.PartNum,            o => o.MapFrom(s => s.Question != null && s.Question.Part != null
                                                          ? (int)s.Question.Part.PartNum : 0))
                .ForMember(d => d.QuestionGroupId,    o => o.MapFrom(s => s.Question != null ? s.Question.QuestionGroupId : null));

            CreateMap<UserResult, UserResultViewModel>()
                .ForMember(d => d.TestTitle,    o => o.MapFrom(s => s.Test != null ? s.Test.Title : string.Empty))
                .ForMember(d => d.Answers,      o => o.MapFrom(s => s.UserAnswers))
                .ForMember(d => d.UserFullName, o => o.MapFrom(s => s.User != null ? s.User.FullName : null))
                .ForMember(d => d.UserEmail,    o => o.MapFrom(s => s.User != null ? s.User.Email : null));

            CreateMap<CourseLesson, CourseLessonViewModel>()
                .ForMember(d => d.QuizQuestions, o => o.MapFrom(s => s.QuizQuestions.OrderBy(q => q.OrderIndex)));
            CreateMap<CourseLessonEditModel, CourseLesson>();

            CreateMap<QuizQuestion, QuizQuestionAdminViewModel>()
                .ForMember(d => d.OptionText1, o => o.MapFrom(s => s.Option != null ? s.Option.OptionText1 : string.Empty))
                .ForMember(d => d.OptionText2, o => o.MapFrom(s => s.Option != null ? s.Option.OptionText2 : string.Empty))
                .ForMember(d => d.OptionText3, o => o.MapFrom(s => s.Option != null ? s.Option.OptionText3 : string.Empty))
                .ForMember(d => d.OptionText4, o => o.MapFrom(s => s.Option != null ? s.Option.OptionText4 : null))
                .ForMember(d => d.CorrectOption, o => o.MapFrom(s => s.Option != null ? s.Option.CorrectOption : string.Empty));

            CreateMap<CourseSection, CourseSectionViewModel>()
                .ForMember(d => d.LessonCount, o => o.MapFrom(s => s.Lessons.Count))
                .ForMember(d => d.Lessons, o => o.MapFrom(s => s.Lessons.OrderBy(l => l.OrderIndex)));
            CreateMap<CourseSectionEditModel, CourseSection>();

            CreateMap<Course, CourseViewModel>()
                .ForMember(d => d.SectionCount, o => o.MapFrom(s => s.Sections.Count))
                .ForMember(d => d.TotalLessons, o => o.MapFrom(s => s.Sections.SelectMany(sec => sec.Lessons).Count()))
                .ForMember(d => d.EnrollmentCount, o => o.MapFrom(s => s.Enrollments.Count))
                .ForMember(d => d.ReviewCount, o => o.MapFrom(s => s.Reviews.Count))
                .ForMember(d => d.AverageRating, o => o.MapFrom(s => s.Reviews.Count == 0 ? 0 : s.Reviews.Average(r => r.Rating)))
                .ForMember(d => d.Sections, o => o.MapFrom(s => s.Sections.OrderBy(sec => sec.OrderIndex)));
            CreateMap<CourseEditModel, Course>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.CreatedAt, o => o.Ignore())
                .ForMember(d => d.UpdatedAt, o => o.Ignore())
                .ForMember(d => d.Sections, o => o.Ignore())
                .ForMember(d => d.Enrollments, o => o.Ignore())
                .ForMember(d => d.Reviews, o => o.Ignore())
                .ForMember(d => d.ThumbnailUrl, o => o.Ignore());
        }
    }
}
