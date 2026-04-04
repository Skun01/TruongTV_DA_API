using Application.DTOs.CardNotes;
using FluentValidation;

namespace API.Validators.CardNotes;

public class UpsertCardNoteRequestValidator : AbstractValidator<UpsertCardNoteRequest>
{
    public UpsertCardNoteRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .MaximumLength(5000);
    }
}
