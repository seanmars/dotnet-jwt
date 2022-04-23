using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Records;
using WebApp.Services;

namespace WebApp.Controllers;

[ApiController]
[Route("api/auth")]
// [Authorize]
public class AuthenticationController : ControllerBase
{
    private readonly ILogger<AuthenticationController> _logger;
    private readonly AccountService _accountService;

    public AuthenticationController(ILogger<AuthenticationController> logger, AccountService accountService)
    {
        _logger = logger;
        _accountService = accountService;
    }

    [NonAction]
    private async Task<bool> ValidateUser(SignInViewRecord signInViewRecord)
    {
        var result = await _accountService.SignIn(signInViewRecord.Username, signInViewRecord.Password);
        if (!result.Succeeded)
        {
            _logger.LogInformation("SignIn failed: {@Errors}", result.Errors);
        }

        return result.Succeeded;
    }

    [NonAction]
    private async Task<bool> CreateUser(SignUpViewRecord signUpViewRecord)
    {
        if (signUpViewRecord.Password != signUpViewRecord.ConfirmPassword)
        {
            _logger.LogWarning("Create User failed: Passwords do not match");
            return false;
        }

        var result = await _accountService.CreateUser(signUpViewRecord.Email, signUpViewRecord.Username, signUpViewRecord.Password);
        if (!result.Succeeded)
        {
            _logger.LogWarning("Create User failed: {@Errors}", result.Errors);
        }

        return result.Succeeded;
    }

    [AllowAnonymous]
    [HttpPost("signin")]
    public async Task<IActionResult> SignIn([FromBody] SignInViewRecord signInViewRecord)
    {
        if (await ValidateUser(signInViewRecord))
        {
            return Ok();
        }
        else
        {
            return BadRequest();
        }
    }

    [AllowAnonymous]
    [HttpPost("signup")]
    public async Task<IActionResult> SignUp([FromBody] SignUpViewRecord signUpViewRecord)
    {
        if (await CreateUser(signUpViewRecord))
        {
            return Ok();
        }
        else
        {
            return BadRequest();
        }
    }

    [Route("/api/claims")]
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult GetClaims()
    {
        var claims = User.Claims.Select(p => new { p.Type, p.Value });
        return Ok(claims);
    }
}