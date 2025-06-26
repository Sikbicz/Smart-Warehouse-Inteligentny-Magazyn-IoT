namespace USERSERVICE.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using USERSERVICE.Data;
using USERSERVICE.Models; 
using USERSERVICE.Models.Requests;
using BCrypt.Net;

public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;
    private readonly ApplicationDbContext _dbContext;

    public UserService(ILogger<UserService> logger, ApplicationDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<bool> RegisterUserAsync(RegisterUserRequest model)
    {
        if (await _dbContext.Users.AnyAsync(u => u.Email == model.Email))
        {
            _logger.LogWarning("Użytkownik o tym e-mailu już istnieje.");
            return false;
        }

        var passwordHash = BCrypt.HashPassword(model.Password);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = model.Email,
            PasswordHash = passwordHash,
            Role = UserRole.Pracownik,
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();
        
        _logger.LogInformation("Użytkownik {Email} zarejestrowany pomyślnie w bazie danych.", model.Email);
        return true;
    }
    

    public async Task<string?> LoginUserAsync(LoginUserRequest model)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

        if (user == null || !BCrypt.Verify(model.Password, user.PasswordHash))
        {
            _logger.LogWarning("Nieudane logowanie dla {Email}.", model.Email);
            return null;
        }

        _logger.LogInformation("Logowanie pomyślne dla {Email}.", model.Email);
        return $"auth-token-for-{user.Email}";
    }

    public async Task<bool> InitiatePasswordResetAsync(string email)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return false;

        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        user.PasswordResetToken = token;
        user.PasswordResetTokenExpiresAt = DateTime.UtcNow.AddHours(1);

        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Wygenerowano token resetu hasła dla {Email}. Token: {Token}", email, token);
        return true;
    }

    public async Task<bool> CompletePasswordResetAsync(CompletePasswordResetRequest model)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => 
            u.Email == model.Email && 
            u.PasswordResetToken == model.Token && 
            u.PasswordResetTokenExpiresAt > DateTime.UtcNow);

        if (user == null)
        {
            _logger.LogWarning("Nieudany reset hasła dla {Email} - nieprawidłowy token lub token wygasł.", model.Email);
            return false;
        }

        user.PasswordHash = BCrypt.HashPassword(model.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiresAt = null;

        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Hasło dla {Email} zostało pomyślnie zresetowane.", model.Email);
        return true;
    }
}
