using Application.DTOs.Exams;
using Domain.Constants;
using Domain.Enums;

namespace Application.Helper;

public static class ExamImportHelper
{
    public static ExamImportTemplateGuide CreateGuide()
    {
        return new ExamImportTemplateGuide
        {
            Overview = new List<string>
            {
                "Import creates a brand-new exam only. Existing exams are never updated.",
                "Imported exams always start with status Draft.",
                "The payload should contain the full tree: exam -> sections -> questionGroups -> questions -> options.",
                "audioUrl, imageUrl, and audioScript are kept as reference values only. No media copy or TTS generation runs during import.",
            },
            AllowedValues =
            {
                ["level"] = ImportTemplateGuideHelper.EnumValues<JlptLevel>(),
                ["sectionType"] = ImportTemplateGuideHelper.EnumValues<SectionType>(),
                ["mondaiType"] = ImportTemplateGuideHelper.EnumValues<ChoukaiMondaiType>(),
                ["optionLabel"] = ImportTemplateGuideHelper.EnumValues<OptionLabel>(),
                ["optionType"] = ImportTemplateGuideHelper.EnumValues<OptionType>(),
            },
            RulesByNode =
            {
                ["exam"] = new List<string>
                {
                    "title is required and must be at most 500 characters.",
                    "level is required and must be a valid JlptLevel value.",
                    "totalDurationMinutes is required and must be > 0 and <= 300.",
                    "sections is required and must contain at least one section.",
                },
                ["section"] = new List<string>
                {
                    "sectionType is required and must be a valid SectionType value.",
                    "orderIndex must be >= 0 and unique among sibling sections.",
                    "durationMinutes must be > 0.",
                    "maxScore must be > 0.",
                    "passScore must be >= 0 and <= maxScore.",
                    "questionGroups is required and must contain at least one group.",
                },
                ["questionGroup"] = new List<string>
                {
                    "instruction is required and must be at most 2000 characters.",
                    "orderIndex must be >= 0 and unique among sibling groups.",
                    "passageText is optional and must be at most 10000 characters.",
                    "audioUrl is optional and must be at most 512 characters.",
                    "audioScript is optional and must be at most 10000 characters.",
                    "mondaiType is optional and mainly used for Choukai groups.",
                    "questions is required and must contain at least one question.",
                },
                ["question"] = new List<string>
                {
                    "questionText is required and must be at most 5000 characters.",
                    "orderIndex must be >= 0 and unique among sibling questions.",
                    "score is required and must be > 0.",
                    "imageUrl is optional and must be at most 512 characters.",
                    "imageCaption is optional and must be at most 1000 characters.",
                    "explanation is optional and must be at most 5000 characters.",
                    "options is required and must contain from 2 to 4 items.",
                },
                ["option"] = new List<string>
                {
                    "label is required, must be one of A/B/C/D, and must be unique within the question.",
                    "optionType is required and must be one of Text/Image/TextAndImage.",
                    "Exactly one option in each question must have isCorrect = true.",
                    "text is optional and must be at most 2000 characters.",
                    "imageUrl is optional and must be at most 512 characters.",
                },
            },
        };
    }

    public static ImportExamRequest CreateTemplate()
    {
        return new ImportExamRequest
        {
            Title = "JLPT N5 Mock Test 01",
            Level = "N5",
            TotalDurationMinutes = 120,
            Sections = new List<ImportExamSectionRequest>
            {
                new()
                {
                    SectionType = "Moji",
                    OrderIndex = 0,
                    DurationMinutes = 25,
                    MaxScore = 60,
                    PassScore = 19,
                    QuestionGroups = new List<ImportQuestionGroupRequest>
                    {
                        new()
                        {
                            Instruction = "Choose the correct answer.",
                            OrderIndex = 0,
                            Questions = new List<ImportExamQuestionRequest>
                            {
                                new()
                                {
                                    QuestionText = "What is the correct reading of 食べる?",
                                    Score = 1,
                                    OrderIndex = 0,
                                    Options = new List<ImportQuestionOptionRequest>
                                    {
                                        new()
                                        {
                                            Label = "A",
                                            Text = "たべる",
                                            OptionType = "Text",
                                            IsCorrect = true,
                                        },
                                        new()
                                        {
                                            Label = "B",
                                            Text = "のめる",
                                            OptionType = "Text",
                                            IsCorrect = false,
                                        },
                                    },
                                },
                            },
                        },
                    },
                },
            },
        };
    }

    public static ExamImportCommitResponse BuildBlockedCommitResponse(ExamImportPreviewResponse preview)
    {
        return new ExamImportCommitResponse
        {
            IsSuccess = false,
            HasValidationErrors = true,
            Action = "skipped",
            Title = preview.Item.Title,
            SectionsCount = preview.Item.SectionsCount,
            QuestionGroupsCount = preview.Item.QuestionGroupsCount,
            QuestionsCount = preview.Item.QuestionsCount,
            OptionsCount = preview.Item.OptionsCount,
            Errors = preview.Item.IsValid
                ? new List<string> { MessageConstants.ExamMessage.IMPORT_BATCH_HAS_ERRORS }
                : preview.Item.Errors.ToList(),
        };
    }

    public static ExamImportPreviewItemResponse ValidateImport(ImportExamRequest request)
    {
        var previewItem = new ExamImportPreviewItemResponse
        {
            Title = request.Title?.Trim() ?? string.Empty,
            Level = request.Level?.Trim() ?? string.Empty,
            SectionsCount = request.Sections?.Count ?? 0,
            QuestionGroupsCount = request.Sections?.Sum(section => section.QuestionGroups?.Count ?? 0) ?? 0,
            QuestionsCount = request.Sections?.Sum(section => section.QuestionGroups?.Sum(group => group.Questions?.Count ?? 0) ?? 0) ?? 0,
            OptionsCount = request.Sections?.Sum(section => section.QuestionGroups?.Sum(group => group.Questions?.Sum(question => question.Options?.Count ?? 0) ?? 0) ?? 0) ?? 0,
        };

        ValidateRequiredText(request.Title, "title", 500, previewItem.Errors);
        ValidateRequiredEnum<JlptLevel>(request.Level, "level", previewItem.Errors);

        if (request.TotalDurationMinutes <= 0 || request.TotalDurationMinutes > 300)
            previewItem.Errors.Add(BuildFieldCode(MessageConstants.ExamMessage.IMPORT_FIELD_INVALID, "totalDurationMinutes"));

        if (request.Sections == null || request.Sections.Count == 0)
        {
            previewItem.Errors.Add(BuildFieldCode(MessageConstants.ExamMessage.IMPORT_SECTIONS_REQUIRED, "sections"));
        }
        else
        {
            ValidateSections(request.Sections, previewItem.Errors);
        }

        previewItem.IsValid = previewItem.Errors.Count == 0;
        return previewItem;
    }

    private static void ValidateSections(List<ImportExamSectionRequest> sections, List<string> errors)
    {
        var orderIndexes = new HashSet<int>();

        for (var sectionIndex = 0; sectionIndex < sections.Count; sectionIndex++)
        {
            var section = sections[sectionIndex];
            var path = $"sections[{sectionIndex}]";

            ValidateRequiredEnum<SectionType>(section.SectionType, $"{path}.sectionType", errors);
            ValidateNonNegative(section.OrderIndex, $"{path}.orderIndex", errors);

            if (!orderIndexes.Add(section.OrderIndex))
                errors.Add(BuildFieldCode(MessageConstants.ExamMessage.IMPORT_DUPLICATE_ORDER_INDEX, $"{path}.orderIndex"));

            if (section.DurationMinutes <= 0)
                errors.Add(BuildFieldCode(MessageConstants.ExamMessage.IMPORT_FIELD_INVALID, $"{path}.durationMinutes"));

            if (section.MaxScore <= 0)
                errors.Add(BuildFieldCode(MessageConstants.ExamMessage.IMPORT_FIELD_INVALID, $"{path}.maxScore"));

            if (section.PassScore < 0 || section.PassScore > section.MaxScore)
                errors.Add(BuildFieldCode(MessageConstants.ExamMessage.IMPORT_PASS_SCORE_INVALID, $"{path}.passScore"));

            if (section.QuestionGroups == null || section.QuestionGroups.Count == 0)
            {
                errors.Add(BuildFieldCode(MessageConstants.ExamMessage.IMPORT_GROUPS_REQUIRED, $"{path}.questionGroups"));
                continue;
            }

            ValidateGroups(section.QuestionGroups, $"{path}.questionGroups", errors);
        }
    }

    private static void ValidateGroups(List<ImportQuestionGroupRequest> groups, string parentPath, List<string> errors)
    {
        var orderIndexes = new HashSet<int>();

        for (var groupIndex = 0; groupIndex < groups.Count; groupIndex++)
        {
            var group = groups[groupIndex];
            var path = $"{parentPath}[{groupIndex}]";

            ValidateRequiredText(group.Instruction, $"{path}.instruction", 2000, errors);
            ValidateOptionalText(group.PassageText, $"{path}.passageText", 10000, errors);
            ValidateOptionalText(group.AudioUrl, $"{path}.audioUrl", 512, errors);
            ValidateOptionalText(group.AudioScript, $"{path}.audioScript", 10000, errors);
            ValidateNonNegative(group.OrderIndex, $"{path}.orderIndex", errors);
            ValidateOptionalEnum<ChoukaiMondaiType>(group.MondaiType, $"{path}.mondaiType", errors);

            if (!orderIndexes.Add(group.OrderIndex))
                errors.Add(BuildFieldCode(MessageConstants.ExamMessage.IMPORT_DUPLICATE_ORDER_INDEX, $"{path}.orderIndex"));

            if (group.Questions == null || group.Questions.Count == 0)
            {
                errors.Add(BuildFieldCode(MessageConstants.ExamMessage.IMPORT_QUESTIONS_REQUIRED, $"{path}.questions"));
                continue;
            }

            ValidateQuestions(group.Questions, $"{path}.questions", errors);
        }
    }

    private static void ValidateQuestions(List<ImportExamQuestionRequest> questions, string parentPath, List<string> errors)
    {
        var orderIndexes = new HashSet<int>();

        for (var questionIndex = 0; questionIndex < questions.Count; questionIndex++)
        {
            var question = questions[questionIndex];
            var path = $"{parentPath}[{questionIndex}]";

            ValidateRequiredText(question.QuestionText, $"{path}.questionText", 5000, errors);
            ValidateOptionalText(question.ImageUrl, $"{path}.imageUrl", 512, errors);
            ValidateOptionalText(question.ImageCaption, $"{path}.imageCaption", 1000, errors);
            ValidateOptionalText(question.Explanation, $"{path}.explanation", 5000, errors);
            ValidateNonNegative(question.OrderIndex, $"{path}.orderIndex", errors);

            if (!orderIndexes.Add(question.OrderIndex))
                errors.Add(BuildFieldCode(MessageConstants.ExamMessage.IMPORT_DUPLICATE_ORDER_INDEX, $"{path}.orderIndex"));

            if (question.Score <= 0)
                errors.Add(BuildFieldCode(MessageConstants.ExamMessage.IMPORT_FIELD_INVALID, $"{path}.score"));

            if (question.Options == null || question.Options.Count < 2 || question.Options.Count > 4)
            {
                errors.Add(BuildFieldCode(MessageConstants.ExamMessage.IMPORT_OPTIONS_INVALID_COUNT, $"{path}.options"));
                continue;
            }

            ValidateOptions(question.Options, $"{path}.options", errors);
        }
    }

    private static void ValidateOptions(List<ImportQuestionOptionRequest> options, string parentPath, List<string> errors)
    {
        var labels = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var optionIndex = 0; optionIndex < options.Count; optionIndex++)
        {
            var option = options[optionIndex];
            var path = $"{parentPath}[{optionIndex}]";

            ValidateRequiredEnum<OptionLabel>(option.Label, $"{path}.label", errors);
            ValidateRequiredEnum<OptionType>(option.OptionType, $"{path}.optionType", errors);
            ValidateOptionalText(option.Text, $"{path}.text", 2000, errors);
            ValidateOptionalText(option.ImageUrl, $"{path}.imageUrl", 512, errors);

            var normalizedLabel = option.Label?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(normalizedLabel) && !labels.Add(normalizedLabel))
                errors.Add(BuildFieldCode(MessageConstants.ExamMessage.IMPORT_DUPLICATE_OPTION_LABEL, $"{path}.label"));
        }

        var correctCount = options.Count(option => option.IsCorrect);
        if (correctCount != 1)
            errors.Add(BuildFieldCode(MessageConstants.ExamMessage.IMPORT_CORRECT_OPTION_INVALID, parentPath));
    }

    private static void ValidateNonNegative(int value, string fieldName, List<string> errors)
    {
        if (value < 0)
            errors.Add(BuildFieldCode(MessageConstants.ExamMessage.IMPORT_FIELD_INVALID, fieldName));
    }

    private static void ValidateRequiredText(string? value, string fieldName, int maxLength, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add(BuildFieldCode(MessageConstants.ExamMessage.IMPORT_FIELD_REQUIRED, fieldName));
            return;
        }

        if (value.Trim().Length > maxLength)
            errors.Add(BuildFieldCode(MessageConstants.ExamMessage.IMPORT_FIELD_TOO_LONG, fieldName));
    }

    private static void ValidateOptionalText(string? value, string fieldName, int maxLength, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        if (value.Trim().Length > maxLength)
            errors.Add(BuildFieldCode(MessageConstants.ExamMessage.IMPORT_FIELD_TOO_LONG, fieldName));
    }

    private static void ValidateRequiredEnum<TEnum>(string? value, string fieldName, List<string> errors)
        where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add(BuildFieldCode(MessageConstants.ExamMessage.IMPORT_FIELD_REQUIRED, fieldName));
            return;
        }

        if (!Enum.TryParse<TEnum>(value.Trim(), true, out _))
            errors.Add(BuildFieldCode(MessageConstants.ExamMessage.IMPORT_FIELD_INVALID, fieldName));
    }

    private static void ValidateOptionalEnum<TEnum>(string? value, string fieldName, List<string> errors)
        where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        if (!Enum.TryParse<TEnum>(value.Trim(), true, out _))
            errors.Add(BuildFieldCode(MessageConstants.ExamMessage.IMPORT_FIELD_INVALID, fieldName));
    }

    private static string BuildFieldCode(string code, string fieldName)
    {
        return $"{code}:{fieldName}";
    }
}
