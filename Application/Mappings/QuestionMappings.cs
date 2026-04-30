using Application.DTOs.Questions;
using Domain.Entities;

namespace Application.Mappings;

public static class QuestionMappings
{
    public static QuestionResponse ToQuestionResponse(this Question question)
    {
        return new QuestionResponse
        {
            Id = question.Id,
            GroupId = question.GroupId,
            QuestionText = question.QuestionText,
            ImageUrl = question.ImageUrl,
            ImageCaption = question.ImageCaption,
            Explanation = question.Explanation,
            Score = question.Score,
            OrderIndex = question.OrderIndex,
            Options = question.Options
                .OrderBy(o => o.Label)
                .Select(o => o.ToOptionResponse())
                .ToList(),
            CreatedAt = question.CreatedAt,
            UpdatedAt = question.UpdatedAt,
        };
    }

    public static QuestionOptionResponse ToOptionResponse(this QuestionOption option)
    {
        return new QuestionOptionResponse
        {
            Id = option.Id,
            Label = option.Label.ToString(),
            Text = option.Text,
            ImageUrl = option.ImageUrl,
            OptionType = option.OptionType.ToString(),
            IsCorrect = option.IsCorrect,
        };
    }
}
