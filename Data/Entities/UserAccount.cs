using Microsoft.AspNetCore.Identity;

namespace Data.Entities;

public class UserAccount : IdentityUser
{
    [ProtectedPersonalData]
    public string FirstName { get; set; } = null!;

    [ProtectedPersonalData]
    public string LastName { get; set; } = null!;

    [ProtectedPersonalData]
    public string? Bio { get; set; }

    [ProtectedPersonalData]
    public string ProfileImage { get; set; } = "";

    public string? AddressId { get; set; }
    public UserAddress? Address { get; set; }
}
