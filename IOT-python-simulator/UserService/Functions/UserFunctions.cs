using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using USERSERVICE.Models.Requests;
using USERSERVICE.Services;

namespace USERSERVICE.Functions;

public class UserFunctions
{
    private readonly ILogger<UserFunctions> _logger;
    private readonly IUserService _userService;

    public UserFunctions(ILogger<UserFunctions> logger, IUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    [Function("RegisterUser")]
    public async Task<HttpResponseData> Register([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "users/register")] HttpRequestData req)
    {
        var request = await req.ReadFromJsonAsync<RegisterUserRequest>();
        if (request == null) return req.CreateResponse(HttpStatusCode.BadRequest);

        var success = await _userService.RegisterUserAsync(request);
        return req.CreateResponse(success ? HttpStatusCode.Created : HttpStatusCode.Conflict);
    }

    [Function("LoginUser")]
    public async Task<HttpResponseData> Login([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "users/login")] HttpRequestData req)
    {
        var request = await req.ReadFromJsonAsync<LoginUserRequest>();
        if (request == null) return req.CreateResponse(HttpStatusCode.BadRequest);

        var token = await _userService.LoginUserAsync(request);
        if (token == null) return req.CreateResponse(HttpStatusCode.Unauthorized);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(new { Token = token });
        return response;
    }

    [Function("InitiatePasswordReset")]
    public async Task<HttpResponseData> InitiatePasswordReset([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "users/password-reset/initiate")] HttpRequestData req)
    {
        var request = await req.ReadFromJsonAsync<InitiatePasswordResetRequest>();
        if (request == null) return req.CreateResponse(HttpStatusCode.BadRequest);
        
        await _userService.InitiatePasswordResetAsync(request.Email);
        return req.CreateResponse(HttpStatusCode.OK);
    }

    [Function("CompletePasswordReset")]
    public async Task<HttpResponseData> CompletePasswordReset([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "users/password-reset/complete")] HttpRequestData req)
    {
        var request = await req.ReadFromJsonAsync<CompletePasswordResetRequest>();
        if (request == null) return req.CreateResponse(HttpStatusCode.BadRequest);

        var success = await _userService.CompletePasswordResetAsync(request);
        return req.CreateResponse(success ? HttpStatusCode.OK : HttpStatusCode.BadRequest);
    }
}
