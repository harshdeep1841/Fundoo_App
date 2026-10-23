using BusinessLayer.Interface;
using BusinessLayer.Service.PasswordHandler;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Models;
using RepositoryLayer.Entity;
using RepositoryLayer.Interface;
using Middleware.Service;
using Model.EmailModel;

namespace BusinessLayer.Service;

public class UserBL : IUserBL
{
    private readonly IUserRL userRL;
    private readonly JwtService jwtService;
    private readonly EmailClient client;
    private readonly ILogger<UserBL> logger;
    private readonly PasswordService passwordService;

    public UserBL(
        IUserRL userRL,
        JwtService jwtService,
        EmailClient client,
        ILogger<UserBL> logger,
        PasswordService passwordService)
    {
        this.userRL = userRL;
        this.jwtService = jwtService;
        this.client = client;
        this.logger = logger;
        this.passwordService = passwordService;
    }

    // Registration business logic
    public Task<ResponseModel<string>> RegisterUserBL(
        RegistrationModel model)
    {
        var (hash, salt) =
            passwordService.HashPassword(model.Password);

        var user = new UserEntity
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            PhoneNumber = model.ContactNumber,
            Password = hash,
            Salt = salt
        };

        userRL.RegisterUserRL(user);

        return Task.FromResult(new ResponseModel<string>
        {
            IsSuccess = true,
            Message = "Registration successful.",
            Data = user.Email
        });
    }

    // Login business logic
    public ResponseModel<LoginResponseModel> LoginUserBL(
        LoginModel loginModel)
    {
        UserEntity? user =
            userRL.FindUserByEmail(loginModel.Email);

        if (user == null)
        {
            return new ResponseModel<LoginResponseModel>
            {
                IsSuccess = false,
                Message = "Invalid email or password.",
                Data = null
            };
        }

        bool isPasswordValid = passwordService.VerifyPassword(
            loginModel.Password,
            user.Password,
            user.Salt);

        if (!isPasswordValid)
        {
            return new ResponseModel<LoginResponseModel>
            {
                IsSuccess = false,
                Message = "Invalid email or password.",
                Data = null
            };
        }

        return new ResponseModel<LoginResponseModel>
        {
            IsSuccess = true,
            Message = "Login successful.",
            Data = new LoginResponseModel
            {
                Email = user.Email,
                Token = jwtService.GenerateToken(user)
            }
        };
    }

    // Forget-password business logic
    public async Task<ResponseModel<string>> ForgetPasswordBL(
        ForgetPasswordModel model)
    {
        UserEntity? user =
            userRL.FindUserByEmail(model.Email);

        // Generic response helps avoid exposing whether an account exists.
        var genericResponse = new ResponseModel<string>
        {
            IsSuccess = true,
            Message = "If an account exists, password reset instructions will be sent.",
            Data = null
        };

        if (user == null)
        {
            return genericResponse;
        }

        var token = jwtService.GenerateResetToken(user);

        var emailModel = new EmailModel
        {
            To = user.Email,
            Subject = "Fundoo Password Reset",
            Body = $"""
                    Your password reset token is:
                    {token}

                    This token will expire in 15 minutes.
                    """
        };

        bool emailSent = await client.SendEmail(emailModel);

        if (!emailSent)
        {
            logger.LogWarning(
                "Password reset email could not be sent.");

            // Keep the external response generic.
        }

        return genericResponse;
    }

    // Reset-password business logic
    public Task<ResponseModel<string>> ResetPassword(
        ResetPasswordModel model)
    {
        if (model.NewPassword != model.ConfirmPassword)
        {
            return Task.FromResult(new ResponseModel<string>
            {
                IsSuccess = false,
                Message = "Passwords do not match.",
                Data = null
            });
        }

        int userId;

        try
        {
            userId =
                jwtService.ValidatePasswordResetToken(model.Token);
        }
        catch (SecurityTokenExpiredException)
        {
            return Task.FromResult(new ResponseModel<string>
            {
                IsSuccess = false,
                Message = "Reset token has expired.",
                Data = null
            });
        }
        catch (SecurityTokenException)
        {
            return Task.FromResult(new ResponseModel<string>
            {
                IsSuccess = false,
                Message = "Invalid reset token.",
                Data = null
            });
        }

        UserEntity? user = userRL.GetUserById(userId);

        if (user == null)
        {
            return Task.FromResult(new ResponseModel<string>
            {
                IsSuccess = false,
                Message = "User not found.",
                Data = null
            });
        }

        // Use the same hash-and-salt mechanism as registration/login.
        var (hash, salt) =
            passwordService.HashPassword(model.NewPassword);

        user.Password = hash;
        user.Salt = salt;

        userRL.UpdatePassword(user);

        return Task.FromResult(new ResponseModel<string>
        {
            IsSuccess = true,
            Message = "Your password has been reset successfully.",
            Data = null
        });
    }

    // Google login business logic (unchanged)
    public ResponseModel<LoginResponseModel> GoogleLoginBL(
        string email,
        string providerSubject,
        string firstName,
        string lastName)
    {
        try
        {
            UserEntity? user =
                userRL.FindUserByExternalLogin(
                    "Google",
                    providerSubject);

            if (user == null)
            {
                user = userRL.CreateGoogleUserWithExternalLogin(
                    email,
                    firstName,
                    lastName,
                    providerSubject);
            }

            return new ResponseModel<LoginResponseModel>
            {
                IsSuccess = true,
                Message = "Google login successful.",
                Data = new LoginResponseModel
                {
                    Email = user.Email,
                    Token = jwtService.GenerateToken(user)
                }
            };
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(
                ex, "Google login could not be completed.");

            return new ResponseModel<LoginResponseModel>
            {
                IsSuccess = false,
                Message = ex.Message,
                Data = null
            };
        }
    }
}