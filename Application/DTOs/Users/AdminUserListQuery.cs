using Application.DTOs.Common;

namespace Application.DTOs.Users;

public class AdminUserListQuery : PagingQuery
{
    public string? Q { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsVerified { get; set; }
}
