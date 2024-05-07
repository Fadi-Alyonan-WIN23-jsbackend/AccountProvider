namespace AccountProvider.Models;

public class UserSignInModel
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public bool RememberMe { get; set;}

}
