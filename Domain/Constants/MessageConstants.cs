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

    public static class GrammarMessage
    {
        public const string CARD_NOT_FOUND = "Grammar_CardNotFound_404";
        public const string DETAIL_NOT_FOUND = "Grammar_DetailNotFound_404";
        public const string READ_FORBIDDEN = "Grammar_ReadForbidden_401";
        public const string INVALID_RELATION = "Grammar_InvalidRelation_400";
        public const string RELATED_CARD_NOT_FOUND = "Grammar_RelatedCardNotFound_404";
        public const string INVALID_RICH_TEXT = "Grammar_InvalidRichText_400";
        public const string IMPORT_INVALID_PAYLOAD = "Grammar_ImportInvalidPayload_400";
        public const string IMPORT_BATCH_HAS_ERRORS = "Grammar_ImportBatchHasErrors_400";
        public const string IMPORT_ROW_NUMBER_INVALID = "Grammar_ImportRowNumberInvalid_400";
        public const string IMPORT_FIELD_REQUIRED = "Grammar_ImportFieldRequired_400";
        public const string IMPORT_FIELD_TOO_LONG = "Grammar_ImportFieldTooLong_400";
        public const string IMPORT_FIELD_INVALID = "Grammar_ImportFieldInvalid_400";
        public const string IMPORT_LIST_TOO_MANY_ITEMS = "Grammar_ImportListTooManyItems_400";
        public const string IMPORT_RELATED_GRAMMAR_NOT_FOUND = "Grammar_ImportRelatedGrammarNotFound_404";
        public const string IMPORT_DUPLICATE_RELATION = "Grammar_ImportDuplicateRelation_400";
        public const string IMPORT_SENTENCES_TOO_MANY = "Grammar_ImportSentencesTooMany_400";
        public const string IMPORT_SENTENCE_ID_NOT_ALLOWED = "Grammar_ImportSentenceIdNotAllowed_400";
        public const string IMPORT_SPEAKER_ID_INVALID = "Grammar_ImportSpeakerIdInvalid_400";
        public const string IMPORT_SPEAKER_ID_NOT_SUPPORTED = "Grammar_ImportSpeakerIdNotSupported_400";
    }

    public static class KanjiMessage
    {
        public const string CARD_NOT_FOUND = "Kanji_CardNotFound_404";
        public const string DETAIL_NOT_FOUND = "Kanji_DetailNotFound_404";
        public const string READ_FORBIDDEN = "Kanji_ReadForbidden_401";
        public const string KANJI_ALREADY_EXISTS = "Kanji_KanjiAlreadyExists_409";
        public const string IMPORT_INVALID_PAYLOAD = "Kanji_ImportInvalidPayload_400";
        public const string IMPORT_BATCH_HAS_ERRORS = "Kanji_ImportBatchHasErrors_400";
        public const string IMPORT_DUPLICATE_KANJI_IN_BATCH = "Kanji_ImportDuplicateKanjiInBatch_400";
        public const string IMPORT_KANJI_ALREADY_EXISTS = "Kanji_ImportKanjiAlreadyExists_400";
        public const string IMPORT_ROW_NUMBER_INVALID = "Kanji_ImportRowNumberInvalid_400";
        public const string IMPORT_FIELD_REQUIRED = "Kanji_ImportFieldRequired_400";
        public const string IMPORT_FIELD_TOO_LONG = "Kanji_ImportFieldTooLong_400";
        public const string IMPORT_FIELD_INVALID = "Kanji_ImportFieldInvalid_400";
        public const string IMPORT_LIST_TOO_MANY_ITEMS = "Kanji_ImportListTooManyItems_400";
        public const string IMPORT_RADICALS_REQUIRED = "Kanji_ImportRadicalsRequired_400";
        public const string IMPORT_DUPLICATE_RADICAL_IN_ITEM = "Kanji_ImportDuplicateRadicalInItem_400";
    }

    public static class AuthMessage
    {
        public const string INVALID_LOGIN = "Invalid_400";
        public const string EMAIL_EXIST = "Email_Exist_409";
        public const string TOKEN_EXPIRED = "Token_Expired_409";
        public const string WRONG_CURRENT_PASSWORD = "Wrong_Current_Password_400";
    }

    public static class DeckMessage
    {
        public const string NOT_FOUND = "Deck_NotFound_404";
        public const string READ_FORBIDDEN = "Deck_Forbidden_403";
        public const string FORK_SOURCE_INVALID = "Deck_ForkSourceInvalid_400";
        public const string FOLDER_NOT_FOUND = "Deck_FolderNotFound_404";
        public const string CARD_NOT_FOUND = "Deck_CardNotFound_404";
        public const string CARD_DUPLICATED_IN_DECK = "Deck_CardDuplicatedInDeck_400";
        public const string INVALID_REORDER_PAYLOAD = "Deck_InvalidReorderPayload_400";
    }

    public static class DeckTypeMessage
    {
        public const string NOT_FOUND = "DeckType_NotFound_404";
        public const string NAME_EXISTS = "DeckType_NameExists_409";
        public const string IN_USE = "DeckType_InUse_400";
    }

    public static class LearningMessage
    {
        public const string CARD_NOT_FOUND = "Learning_CardNotFound_404";
        public const string SESSION_NOT_FOUND = "Learning_SessionNotFound_404";
        public const string SENTENCE_NOT_ATTACHED = "Learning_SentenceNotAttached_404";
        public const string SESSION_COMPLETED = "Learning_SessionCompleted_400";
        public const string INVALID_MODE = "Learning_InvalidMode_400";
        public const string INVALID_SCOPE = "Learning_InvalidScope_400";
        public const string CARD_NOT_IN_SESSION = "Learning_CardNotInSession_400";
        public const string INVALID_SUBMISSION = "Learning_InvalidSubmission_400";
        public const string NO_CARDS_AVAILABLE = "Learning_NoCardsAvailable_400";
    }

    public static class FileUploadMessage
    {
        public const string CLOUDINARY_UPLOAD_FAILED = "FileUpload_CloudinaryUploadFailed_500";
        public const string CLOUDINARY_DELETE_FAILED = "FileUpload_CloudinaryDeleteFailed_500";
        public const string CLOUDINARY_CLOUD_NAME_NOT_CONFIGURED = "FileUpload_CloudinaryCloudNameNotConfigured_500";
        public const string CLOUDINARY_API_KEY_NOT_CONFIGURED = "FileUpload_CloudinaryApiKeyNotConfigured_500";
        public const string CLOUDINARY_API_SECRET_NOT_CONFIGURED = "FileUpload_CloudinaryApiSecretNotConfigured_500";
        public const string INVALID_UPLOAD_REQUEST = "FileUpload_InvalidUploadRequest_400";
    }
}
