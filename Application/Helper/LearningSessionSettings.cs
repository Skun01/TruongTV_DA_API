using Domain.Enums;

namespace Application.Helper;

public sealed record LearningSessionSettings(
    FlashcardContentType FlashcardFront,
    FlashcardContentType FlashcardBack,
    MultipleChoiceQuestionType MultipleChoiceQuestion,
    bool ShuffleOptions);
