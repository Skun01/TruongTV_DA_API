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

    public static class AuthMessage
    {
        public const string INVALID_LOGIN = "Invalid_400";
        public const string EMAIL_EXIST = "Email_Exist_409";
        public const string TOKEN_EXPIRED = "Token_Expired_409";
        public const string WRONG_CURRENT_PASSWORD = "Wrong_Current_Password_400";
    }
}
