namespace Domain.Constants;

public static class MessageConstants
{
    public static class CommonMessage
    {
        public const string INTERNAL_SERVER_ERROR = "Common_505";   // Lỗi Server
        public const string NOT_FOUND = "Common_404"; // không tìm thấy
        public const string INVALID = "Common_400"; // không hợp lệ
        public const string UNAUTHORIZED = "Common_401"; // không cho có authorize
        public const string NOT_ALLOW = "Common_405"; // không được quyền
    }

    public static class AuthMessage
    {
        public const string INVALID_LOGIN = "Invalid_400";
        public const string EMAIL_EXIST = "Email_Exist_409";
        public const string TOKEN_EXPIRED = "Token_Expried_409";
    }

    public static class LearnMessage
    {
        public const string ALREADY_IN_QUEUE = "Queue_Exist_409";
        public const string NOT_IN_QUEUE = "Queue_NotFound_404";
        public const string NO_CARDS_TO_LEARN = "Learn_Empty_404";
        public const string ALREADY_LEARNED = "Learn_Already_409";
    }

    public static class ReviewMessage
    {
        public const string NO_REVIEWS_DUE = "Review_Empty_404";
    }
}
