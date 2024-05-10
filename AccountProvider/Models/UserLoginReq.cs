

namespace AccountProvider.Models;

public class UserLoginReq
{
    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public bool IsPersistent { get; set; }
}
