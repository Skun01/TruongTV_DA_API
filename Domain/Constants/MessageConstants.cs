namespace Domain.Constants;

public static class MessageConstants
{
    public static class CommonMessage
    {
        public const string INTERNAL_SERVER_ERROR = "Common_500";   // Lỗi Server
        public const string NOT_FOUND = "Common_404"; // không tìm thấy
        public const string INVALID = "Common_400"; // không hợp lệ
        public const string UNAUTHORIZED = "Common_401"; // không được quyền
    }

    public static class VocabularyMessage
    {
        public const string CARD_NOT_FOUND = "Vocabulary_CardNotFound_404";
        public const string DETAIL_NOT_FOUND = "Vocabulary_DetailNotFound_404";
        public const string READ_FORBIDDEN = "Vocabulary_ReadForbidden_401";
        public const string AUDIO_SYNTHESIS_FAILED = "Vocabulary_AudioSynthesisFailed_500";
        public const string IMPORT_INVALID_PAYLOAD = "Vocabulary_ImportInvalidPayload_400";
        public const string IMPORT_EXISTING_CARD_NOT_FOUND = "Vocabulary_ImportExistingCardNotFound_404";
        public const string IMPORT_BATCH_HAS_ERRORS = "Vocabulary_ImportBatchHasErrors_400";
        public const string IMPORT_DUPLICATE_WRITING_IN_BATCH = "Vocabulary_ImportDuplicateWritingInBatch_400";
        public const string IMPORT_WRITING_ALREADY_EXISTS = "Vocabulary_ImportWritingAlreadyExists_400";
        public const string IMPORT_ROW_NUMBER_INVALID = "Vocabulary_ImportRowNumberInvalid_400";
        public const string IMPORT_FIELD_REQUIRED = "Vocabulary_ImportFieldRequired_400";
        public const string IMPORT_FIELD_TOO_LONG = "Vocabulary_ImportFieldTooLong_400";
        public const string IMPORT_FIELD_INVALID = "Vocabulary_ImportFieldInvalid_400";
        public const string IMPORT_SPEAKER_ID_INVALID = "Vocabulary_ImportSpeakerIdInvalid_400";
        public const string IMPORT_SPEAKER_ID_NOT_SUPPORTED = "Vocabulary_ImportSpeakerIdNotSupported_400";
        public const string IMPORT_LIST_TOO_MANY_ITEMS = "Vocabulary_ImportListTooManyItems_400";
        public const string IMPORT_MEANINGS_REQUIRED = "Vocabulary_ImportMeaningsRequired_400";
        public const string IMPORT_DEFINITIONS_REQUIRED = "Vocabulary_ImportDefinitionsRequired_400";
        public const string IMPORT_SENTENCES_TOO_MANY = "Vocabulary_ImportSentencesTooMany_400";
        public const string IMPORT_SENTENCE_ID_NOT_ALLOWED = "Vocabulary_ImportSentenceIdNotAllowed_400";
    }

    public static class SentenceMessage
    {
        public const string NOT_FOUND = "Sentence_NotFound_404";
        public const string AUDIO_SYNTHESIS_FAILED = "Sentence_AudioSynthesisFailed_500";
        public const string IMPORT_INVALID_PAYLOAD = "Sentence_ImportInvalidPayload_400";
        public const string IMPORT_BATCH_HAS_ERRORS = "Sentence_ImportBatchHasErrors_400";
        public const string IMPORT_ROW_NUMBER_INVALID = "Sentence_ImportRowNumberInvalid_400";
        public const string IMPORT_FIELD_REQUIRED = "Sentence_ImportFieldRequired_400";
        public const string IMPORT_FIELD_TOO_LONG = "Sentence_ImportFieldTooLong_400";
        public const string IMPORT_FIELD_INVALID = "Sentence_ImportFieldInvalid_400";
        public const string IMPORT_SPEAKER_ID_INVALID = "Sentence_ImportSpeakerIdInvalid_400";
        public const string IMPORT_SPEAKER_ID_NOT_SUPPORTED = "Sentence_ImportSpeakerIdNotSupported_400";
    }

    public static class AuthMessage
    {
        public const string INVALID_LOGIN = "Invalid_400";
        public const string EMAIL_EXIST = "Email_Exist_409";
        public const string TOKEN_EXPIRED = "Token_Expired_409";
        public const string WRONG_CURRENT_PASSWORD = "Wrong_Current_Password_400";
    }
}
