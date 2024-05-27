using Newtonsoft.Json;

namespace AccountProvider.Models;

public class UserRoleModel
{
    public string UserId { get; set; } = null!;
    public string? Role { get; set; }
}
