using Application.DTOs.Conversations;
using FluentValidation;

namespace API.Validators.Conversations;

public class SendMessageRequestValidator : AbstractValidator<SendMessageRequest>
{
    public SendMessageRequestValidator()
    {
        RuleFor(x => x.UserMessage)
            .NotEmpty().WithMessage("User message is required.")
            .MaximumLength(2000).WithMessage("User message must not exceed 2000 characters.");
    }
}
