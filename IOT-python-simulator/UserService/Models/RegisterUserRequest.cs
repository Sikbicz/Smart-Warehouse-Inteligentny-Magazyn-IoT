namespace USERSERVICE.Models.Requests;

public class RegisterUserRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}