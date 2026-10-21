using BusinessLayer.Interface;
using BusinessLayer.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using NLog;

namespace Fundoo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FundooController : ControllerBase
{
    private IUserBL userBL;
    private static readonly Logger logger =
        LogManager.GetCurrentClassLogger();
    public FundooController(IUserBL userBL)
    {
        this.userBL = userBL;
    }

    [HttpPost]
    [Route("register")]
    public IActionResult RegisterUser(RegistrationModel registrationModel)
    {
        ResponseModel<string> response =   userBL.RegisterUserBL(registrationModel);
        if (response.IsSuccess)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }

    [HttpPost]
    [Route("login")]
    public IActionResult LoginUserBL(LoginModel login)
    {
        ResponseModel<LoginResponseModel> response = userBL.LoginUserBL(login);
        if (response.IsSuccess)
        {
            return Ok(response);
        }
        else
        {
            logger.Error("TEST ERROR LOG");
            logger.Info("TEST INFO LOG");

            return BadRequest(response);
        }
      
    }

    [HttpGet]
    public void Default()
    {
       // logger.Fatal("fatal log");
       logger.Info("info log");
        Console.WriteLine("Application is running");
    }
    
    [Authorize]
    [HttpGet]
    [Route("profile")]
    public IActionResult GetProfile()
    {
        Console.WriteLine("Profile is displayed");
        return Ok("Profile is displayed");
        
    }
    
    // [Authorize]
    [HttpPost]
    [Route("forget-password")]
    public async Task<IActionResult> ForgetPassword(ForgetPasswordModel forgetPasswordModel)
    {
        ResponseModel<string> responseModel = await userBL.ForgetPasswordBL(forgetPasswordModel);
        if(responseModel.IsSuccess)
        {
            return Ok(responseModel);
        }
        return BadRequest(responseModel);
    }

    // [Authorize]
    [HttpPost]
    [Route("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordModel resetPasswordModel)
    {
        ResponseModel<string> responseModel = await userBL.ResetPassword(resetPasswordModel);
        if (responseModel.IsSuccess)
        {
            return Ok(responseModel);
        }
        return BadRequest(responseModel);
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
                return Unauthorized("Google identity or verified email is missing.");
            }

            var response = userBL.GoogleLoginBL(
                email,
                googleSubject,
                firstName,
                lastName);

            if (!response.IsSuccess)
            {
                return Conflict(response);
            }

            return Ok(response);
        }
        finally
        {
            await HttpContext.SignOutAsync("ExternalCookie");
        }
    }
}

