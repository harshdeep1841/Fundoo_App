using BusinessLayer.Commands.Users;
using BusinessLayer.Queries.Users;
using BusinessLayer.Interface;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using NLog;
using Models;
using System.Security.Claims;

namespace Fundoo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FundooController : ControllerBase
{
    private readonly IUserBL userBL;
    private readonly ISender sender;

    private static readonly Logger logger =
        LogManager.GetCurrentClassLogger();

    public FundooController(IUserBL userBL, ISender sender)
    {
        this.userBL = userBL;
        this.sender = sender;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser(
        [FromBody] RegistrationModel registrationModel)
    {
        var command = new RegisterUserCommand
        {
            RegisterModel = registrationModel
        };

        var response = await sender.Send(command);

        return response.IsSuccess
            ? Ok(response)
            : BadRequest(response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginUser(
        [FromBody] LoginModel login)
    {
        var query = new LoginUserQuery
        {
            LoginModel = login
        };

        var response = await sender.Send(query);

        if (!response.IsSuccess)
        {
            logger.Warn("Login attempt failed.");
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpGet]
    public IActionResult Default()
    {
        logger.Info("Application is running");
        return Ok("Application is running");
    }

    [Authorize]
    [HttpGet("profile")]
    public IActionResult GetProfile()
    {
        return Ok("Profile is displayed");
    }

    [AllowAnonymous]
    [HttpPost("forget-password")]
    public async Task<IActionResult> ForgetPassword(
        [FromBody] ForgetPasswordModel model)
    {
        var command = new ForgetPasswordCommand
        {
            ForgetPasswordModel = model
        };

        var response = await sender.Send(command);

        return response.IsSuccess
            ? Ok(response)
            : BadRequest(response);
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordModel model)
    {
        var command = new ResetPasswordCommand
        {
            ResetPasswordModel = model
        };

        var response = await sender.Send(command);

        return response.IsSuccess
            ? Ok(response)
            : BadRequest(response);
    }

    [AllowAnonymous]
    [HttpGet("google-login")]
    public IActionResult GoogleLogin()
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = "/api/fundoo/google-complete"
        };

        return Challenge(properties, "GoogleOIDC");
    }

    [AllowAnonymous]
    [HttpGet("google-complete")]
    public async Task<IActionResult> GoogleComplete()
    {
        var result = await HttpContext.AuthenticateAsync(
            "ExternalCookie");

        if (!result.Succeeded || result.Principal == null)
        {
            return Unauthorized("Google sign-in failed.");
        }

        try
        {
            var principal = result.Principal;

            var googleSubject =
                principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? principal.FindFirst("sub")?.Value;

            var email =
                principal.FindFirst(ClaimTypes.Email)?.Value
                ?? principal.FindFirst("email")?.Value;

            var emailVerified =
                principal.FindFirst("email_verified")?.Value;

            var firstName =
                principal.FindFirst(ClaimTypes.GivenName)?.Value
                ?? principal.FindFirst("given_name")?.Value
                ?? string.Empty;

            var lastName =
                principal.FindFirst(ClaimTypes.Surname)?.Value
                ?? principal.FindFirst("family_name")?.Value
                ?? string.Empty;

            if (string.IsNullOrWhiteSpace(googleSubject) ||
                string.IsNullOrWhiteSpace(email) ||
                !string.Equals(
                    emailVerified,
                    "true",
                    StringComparison.OrdinalIgnoreCase))
            {
                return Unauthorized(
                    "Google identity or verified email is missing.");
            }

            // Google login remains in UserBL for now.
            var response = userBL.GoogleLoginBL(
                email,
                googleSubject,
                firstName,
                lastName);

            return response.IsSuccess
                ? Ok(response)
                : Conflict(response);
        }
        finally
        {
            await HttpContext.SignOutAsync("ExternalCookie");
        }
    }
}