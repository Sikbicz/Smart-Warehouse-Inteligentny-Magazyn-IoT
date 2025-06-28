using Microsoft.EntityFrameworkCore;
using NewSmartWarehouseProject.Backend.Data;
using NewSmartWarehouseProject.Backend.Models;
using BCrypt.Net;

namespace NewSmartWarehouseProject.Backend.Services;

public class UserService
{
    private readonly ApplicationDbContext _dbContext;

    public UserService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> RegisterUserAsync(RegisterRequest request)
    {
        if (string.IsNullOrEmpty(request.Password))
        {
            throw new ArgumentException("Password cannot be null or empty.", nameof(request.Password));
        }

        if (await _dbContext.Users.AnyAsync(u => u.Email == request.Email))
        {
            return null; // User with this email already exists
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var newUser = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            Email = request.Email,
            PasswordHash = passwordHash
        };

        _dbContext.Users.Add(newUser);
        await _dbContext.SaveChangesAsync();

        return newUser;
    }

    public async Task<User?> LoginUserAsync(LoginRequest request)
    {
        if (string.IsNullOrEmpty(request.Password))
        {
            return null; // Invalid credentials if password is null or empty
        }

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return null; // Invalid credentials
        }

        return user;
    }
}