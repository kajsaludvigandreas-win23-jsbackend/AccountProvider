namespace AccountProvider.Models;

public class UpdateOneUserModel
{
    public string? Email { get; set; } 
    public string? NewEmail { get; set; } 
    public string? Password { get; set; } 
    public string? NewPassword { get; set; }
    public string? FirstName { get; set; } 
    public string? LastName { get; set; } 
    public string? PhoneNumber { get; set; }

    public string? ProfileImageUri { get; set; }
}
