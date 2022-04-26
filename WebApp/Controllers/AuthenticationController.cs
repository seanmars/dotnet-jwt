using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Authorizations;
using WebApp.Records;
using WebApp.Services;

namespace WebApp.Controllers;

[ApiController]
[Route("api/auth")]
// [Authorize]
public class AuthenticationController : ControllerBase
{
    private readonly ILogger<AuthenticationController> _logger;
    private readonly SignInManager _signInManager;

    public AuthenticationController(ILogger<AuthenticationController> logger,
        SignInManager signInManager)
    {
        _logger = logger;
        _signInManager = signInManager;
    }

    [NonAction]
    private async Task<bool> CreateUser(SignUpViewRecord signUpViewRecord)
    {
        if (signUpViewRecord.Password != signUpViewRecord.ConfirmPassword)
        {
            _logger.LogWarning("Create User failed: Passwords do not match");
            return false;
        }

        var result = await _signInManager.AccountService.CreateUser(
            signUpViewRecord.Email, signUpViewRecord.Username, signUpViewRecord.Password);
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
        var (result, token) = await _signInManager.SignInAsync(signInViewRecord.Username, signInViewRecord.Password);
        if (!result.Succeeded)
        {
            _logger.LogDebug("SignIn failed: {@Errors}", result.Errors);
            return BadRequest();
        }

        return Ok(new { token });
    }


    [AllowAnonymous]
    [HttpPost("signup")]
    public async Task<IActionResult> SignUp([FromBody] SignUpViewRecord signUpViewRecord)
    {
        if (!await CreateUser(signUpViewRecord))
        {
            return BadRequest();
        }

        return Ok();
    }

    [Route("/api/claims")]
    [HttpGet]
    [Authorize]
    public IActionResult GetClaims()
    {
        var claims = User.Claims.Select(p => new { p.Type, p.Value });
        return Ok(claims);
    }

    [Route("/api/test")]
    [HttpGet]
    [Authorize, RolePermissionFilter]
    public IActionResult Test()
    {
        return Ok();
    }
}