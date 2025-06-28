using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using NewSmartWarehouseProject.Backend.Models;
using NewSmartWarehouseProject.Backend.Services;

namespace NewSmartWarehouseProject.Backend;

public class UserFunctions
{
    private readonly ILogger<UserFunctions> _logger;
    private readonly UserService _userService;

    public UserFunctions(ILogger<UserFunctions> logger, UserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    [Function("Register")]
    public async Task<HttpResponseData> Register([HttpTrigger(AuthorizationLevel.Function, "post", Route = "users/register")] HttpRequestData req)
    {
        _logger.LogInformation("UserFunctions: Register request received.");
        try
        {
            var request = await JsonSerializer.DeserializeAsync<RegisterRequest>(req.Body);
            if (request == null)
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                badRequestResponse.Headers.Add("Access-Control-Allow-Origin", "*"); // Add CORS header
                return badRequestResponse;
            }

            var newUser = await _userService.RegisterUserAsync(request);
            if (newUser == null)
            {
                var conflictResponse = req.CreateResponse(HttpStatusCode.Conflict);
                await conflictResponse.WriteStringAsync("User with this email already exists.");
                conflictResponse.Headers.Add("Access-Control-Allow-Origin", "*"); // Add CORS header
                return conflictResponse;
            }

            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(new { Message = "Registration successful", UserId = newUser.Id });
            response.Headers.Add("Access-Control-Allow-Origin", "*"); // Add CORS header
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError($"UserFunctions: Error during registration: {ex.Message}");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            errorResponse.Headers.Add("Access-Control-Allow-Origin", "*"); // Add CORS header
            return errorResponse;
        }
    }

    [Function("Login")]
    public async Task<HttpResponseData> Login([HttpTrigger(AuthorizationLevel.Function, "post", Route = "users/login")] HttpRequestData req)
    {
        _logger.LogInformation("UserFunctions: Login request received.");
        try
        {
            var request = await JsonSerializer.DeserializeAsync<LoginRequest>(req.Body);
            if (request == null)
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                badRequestResponse.Headers.Add("Access-Control-Allow-Origin", "*"); // Add CORS header
                return badRequestResponse;
            }

            var user = await _userService.LoginUserAsync(request);
            if (user == null)
            {
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteStringAsync("Invalid credentials.");
                unauthorizedResponse.Headers.Add("Access-Control-Allow-Origin", "*"); // Add CORS header
                return unauthorizedResponse;
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new { Message = "Login successful", UserId = user.Id, Username = user.Username });
            response.Headers.Add("Access-Control-Allow-Origin", "*"); // Add CORS header
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError($"UserFunctions: Error during login: {ex.Message}");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            errorResponse.Headers.Add("Access-Control-Allow-Origin", "*"); // Add CORS header
            return errorResponse;
        }
    }
}
