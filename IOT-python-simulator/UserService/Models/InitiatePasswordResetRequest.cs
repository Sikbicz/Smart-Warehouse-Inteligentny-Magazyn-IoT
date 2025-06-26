namespace USERSERVICE.Models.Requests;

public class InitiatePasswordResetRequest
{
    public required string Email { get; set; }
}