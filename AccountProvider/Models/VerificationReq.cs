
namespace AccountProvider.Models;

public class VerificationReq
{
    public string Email { get; set; } = null!;

    public string VerificationCode { get; set; } = null!;
}
