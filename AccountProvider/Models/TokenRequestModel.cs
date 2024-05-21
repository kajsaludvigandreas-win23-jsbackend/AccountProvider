namespace AccountProvider.Models;

public class TokenRequestModel
{
    public string UserId { get; set; } = null!;

    public string Email { get; set; } = null!;
}
