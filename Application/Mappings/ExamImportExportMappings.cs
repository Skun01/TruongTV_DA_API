using Application.DTOs.Exams;
using Application.Helper;
using Domain.Entities;
using Domain.Enums;

namespace Application.Mappings;

public static class ExamImportExportMappings
{
    public static ImportExamRequest ToImportRequest(this Exam exam)
    {
        return new ImportExamRequest
        {
            Title = exam.Title,
            Level = exam.Level.ToString(),
            TotalDurationMinutes = exam.TotalDurationMinutes,
            Sections = exam.Sections
                .OrderBy(section => section.OrderIndex)
                .Select(section => section.ToImportRequest())
                .ToList(),
        };
    }

    public static ImportExamSectionRequest ToImportRequest(this ExamSection section)
    {
        return new ImportExamSectionRequest
        {
            SectionType = section.SectionType.ToString(),
            OrderIndex = section.OrderIndex,
            DurationMinutes = section.DurationMinutes,
            MaxScore = section.MaxScore,
            PassScore = section.PassScore,
            QuestionGroups = section.QuestionGroups
                .OrderBy(group => group.OrderIndex)
                .Select(group => group.ToImportRequest())
                .ToList(),
        };
    }

    public static ImportQuestionGroupRequest ToImportRequest(this QuestionGroup group)
    {
        return new ImportQuestionGroupRequest
        {
            PassageText = group.PassageText,
            AudioUrl = group.AudioUrl,
            AudioScript = group.AudioScript,
            Instruction = group.Instruction,
            OrderIndex = group.OrderIndex,
            MondaiType = group.MondaiType?.ToString(),
            Questions = group.Questions
                .OrderBy(question => question.OrderIndex)
                .Select(question => question.ToImportRequest())
                .ToList(),
        };
    }

    public static ImportExamQuestionRequest ToImportRequest(this Question question)
    {
        return new ImportExamQuestionRequest
        {
            QuestionText = question.QuestionText,
            ImageUrl = question.ImageUrl,
            ImageCaption = question.ImageCaption,
            Explanation = question.Explanation,
            Score = question.Score,
            OrderIndex = question.OrderIndex,
            Options = question.Options
                .OrderBy(option => option.Label)
                .Select(option => option.ToImportRequest())
                .ToList(),
        };
    }

    public static ImportQuestionOptionRequest ToImportRequest(this QuestionOption option)
    {
        return new ImportQuestionOptionRequest
        {
            Label = option.Label.ToString(),
            Text = option.Text,
            ImageUrl = option.ImageUrl,
            OptionType = option.OptionType.ToString(),
            IsCorrect = option.IsCorrect,
        };
    }

    public static Exam ToEntity(this ImportExamRequest request, string currentUserId)
    {
        var examId = Guid.NewGuid().ToString();
        var exam = new Exam
        {
            Id = examId,
            Title = request.Title.Trim(),
            Level = EnumParsingHelper.ParseRequired<JlptLevel>(request.Level),
            TotalDurationMinutes = request.TotalDurationMinutes,
            Status = PublishStatus.Draft,
            CreatedBy = currentUserId,
        };

        foreach (var sectionRequest in request.Sections)
        {
            exam.Sections.Add(sectionRequest.ToEntity(examId));
        }

        return exam;
    }

    private static ExamSection ToEntity(this ImportExamSectionRequest request, string examId)
    {
        var sectionId = Guid.NewGuid().ToString();
        var section = new ExamSection
        {
            Id = sectionId,
            ExamId = examId,
            SectionType = EnumParsingHelper.ParseRequired<SectionType>(request.SectionType),
            OrderIndex = request.OrderIndex,
            DurationMinutes = request.DurationMinutes,
            MaxScore = request.MaxScore,
            PassScore = request.PassScore,
        };

        foreach (var groupRequest in request.QuestionGroups)
        {
            section.QuestionGroups.Add(groupRequest.ToEntity(sectionId));
        }

        return section;
    }

    private static QuestionGroup ToEntity(this ImportQuestionGroupRequest request, string sectionId)
    {
        var groupId = Guid.NewGuid().ToString();
        var group = new QuestionGroup
        {
            Id = groupId,
            SectionId = sectionId,
            PassageText = request.PassageText,
            AudioUrl = request.AudioUrl,
            AudioScript = request.AudioScript,
            Instruction = request.Instruction.Trim(),
            OrderIndex = request.OrderIndex,
            MondaiType = EnumParsingHelper.ParseNullable<ChoukaiMondaiType>(request.MondaiType),
        };

        foreach (var questionRequest in request.Questions)
        {
            group.Questions.Add(questionRequest.ToEntity(groupId));
        }

        return group;
    }

    private static Question ToEntity(this ImportExamQuestionRequest request, string groupId)
    {
        var questionId = Guid.NewGuid().ToString();
        var question = new Question
        {
            Id = questionId,
            GroupId = groupId,
            QuestionText = request.QuestionText.Trim(),
            ImageUrl = request.ImageUrl,
            ImageCaption = request.ImageCaption,
            Explanation = request.Explanation,
            Score = request.Score,
            OrderIndex = request.OrderIndex,
        };

        foreach (var optionRequest in request.Options)
        {
            question.Options.Add(optionRequest.ToEntity(questionId));
        }

        return question;
    }

    private static QuestionOption ToEntity(this ImportQuestionOptionRequest request, string questionId)
    {
        return new QuestionOption
        {
            Id = Guid.NewGuid().ToString(),
            QuestionId = questionId,
            Label = EnumParsingHelper.ParseRequired<OptionLabel>(request.Label),
            Text = request.Text,
            ImageUrl = request.ImageUrl,
            OptionType = EnumParsingHelper.ParseRequired<OptionType>(request.OptionType),
            IsCorrect = request.IsCorrect,
        };
    }
}
