namespace USERSERVICE.Models.Requests;

public class CompletePasswordResetRequest
{
    public required string Email { get; set; }
    public required string Token { get; set; }
    public required string NewPassword { get; set; }
}
