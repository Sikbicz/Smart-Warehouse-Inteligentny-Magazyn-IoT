using USERSERVICE.Models.Requests;

namespace USERSERVICE.Services;

public interface IUserService
{
    Task<bool> RegisterUserAsync(RegisterUserRequest model);
    Task<string?> LoginUserAsync(LoginUserRequest model);
    Task<bool> InitiatePasswordResetAsync(string email);
    Task<bool> CompletePasswordResetAsync(CompletePasswordResetRequest model);
}
