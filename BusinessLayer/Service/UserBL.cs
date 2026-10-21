using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BusinessLayer.Interface;
using BusinessLayer.Service.PasswordHandler;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
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
    public IUserRL userRL;
    public JwtService jwtService;
    public EmailClient client;
    public ILogger<UserBL> logger;
    public PasswordService passwordService;
    public UserBL(IUserRL userRL , JwtService jwtService , EmailClient client , ILogger<UserBL> logger, PasswordService passwordService)
    {
        this.userRL = userRL;
        this.jwtService = jwtService;
        this.client = client;
        this.logger = logger;
        this.passwordService = passwordService;
    }
    
    // public ResponseModel<RegistrationModel> RegisterUserBL(RegistrationModel register)
    // {
    //     return userRL.RegisterUserRL(register);
    // }
    
    public ResponseModel<string> RegisterUserBL(RegistrationModel model)
    {
        var (hash, salt) = passwordService.HashPassword(model.Password);

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

        return new ResponseModel<string>
        {
            IsSuccess = true,
            Message = "Registration successful.",
            Data = user.Email
        };
    }

    // public ResponseModel<LoginResponseModel> LoginUserBL(LoginModel login)
    // {
    //    
    //     
    //     UserEntity? userEntity =   userRL.LoginUserRL(login);
    //     ResponseModel<LoginResponseModel> rm = new ResponseModel<LoginResponseModel>();
    //     if (userEntity == null)
    //     {
    //         rm.IsSuccess = false;
    //         rm.Message = "login email or password is wrong";
    //         rm.Data = null;
    //     }
    //     else
    //     {
    //         rm.IsSuccess = true;
    //         rm.Message = "login success";
    //         rm.Data = new LoginResponseModel
    //         {
    //             Email = userEntity.Email,
    //             Token = jwtService.GenerateToken(userEntity)
    //         };
    //     }
    //     return rm;
    // }
    
    
    public ResponseModel<LoginResponseModel> LoginUserBL(
        LoginModel loginModel)
    {
        UserEntity? user = userRL.FindUserByEmail(
            loginModel.Email
        );

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
            user.Salt
        );

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


//     public async  Task<ResponseModel<string>> ForgetPasswordBL(ForgetPasswordModel model)
//     { UserEntity? userEntity =   userRL.FindUserByEmail(model.Email);
//         if (userEntity == null)
//         { return null; }
 //        var tokenBytes = RandomNumberGenerator.GetBytes(32);
//         string token = Convert.ToHexString(tokenBytes);
//         DateTime expireAt = DateTime.UtcNow.AddMinutes(15);
//         string tokenHash = Convert.ToHexString(
//             SHA256.HashData(
//                 Encoding.UTF8.GetBytes(token)
//             )
//         );
//         PasswordResetTokenEntity passwordResetTokenEntity = new PasswordResetTokenEntity()
//         {
//             UserId = userEntity.UserId,
//             ExpiresAt = expireAt,
//             TokenHash =  tokenHash,
//             User =  userEntity
//                 
//         };
//          userRL.SaveResetToken(passwordResetTokenEntity);
//          var email = new EmailModel.Model.EmailModel
//          {
//              To = userEntity.Email,
//              Subject = "Fundoo Password Reset",
//               Body = $"""
//                       Your password reset token is:
//                       {token}
//                       This token will expire in 15 minutes.
//                       """
//           };
//         await client.SendEmail(email);
//         return new ResponseModel<string>
//         {
//             IsSuccess = true,
//             Message = "If an account exists, a password reset link has been sent.",
//             Data = null
//         };
//
//     }


    public async Task<ResponseModel<string>> ForgetPasswordBL(ForgetPasswordModel model)
    {
        UserEntity user = userRL.FindUserByEmail(model.Email);

        if (user == null)
        {
            return new ResponseModel<string>
            {
                IsSuccess = false,
                Message = "User not found.",
                Data = null
            };
        }

        var token = jwtService.GenerateResetToken(user);

        var emailModel = new EmailModel()
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

            return new ResponseModel<string>(
                false,
                "Unable to process the password reset request.",
                null
            );
        }

// Continue with your success response.
     
        return new ResponseModel<string>
        {
            IsSuccess = true,
            Message = "Your password has been reset.",
            Data = token
        };
        
    }


    // public async Task<ResponseModel<string>> ResetPassword(ResetPasswordModel model)
    // {
    //     string tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(model.Token)));
    //     PasswordResetTokenEntity? resetToken =
    //        userRL.TokenExists(tokenHash);
    //
    //     if (resetToken == null)
    //     {
    //         return new ResponseModel<string>
    //         {
    //             IsSuccess = false,
    //             Message = "Invalid reset token.",
    //             Data = null
    //         };
    //     }
    //
    //     if (resetToken.IsUsed)
    //     {
    //         return new ResponseModel<string>
    //         {
    //             IsSuccess = false,
    //             Message = "Reset token has already been used.",
    //             Data = null
    //         };
    //     }
    //
    //     if (resetToken.ExpiresAt < DateTime.UtcNow)
    //     {
    //         return new ResponseModel<string>
    //         {
    //             IsSuccess = false,
    //             Message = "Reset token has expired.",
    //             Data = null
    //         };
    //     }
    //     
    //     
    //     
    //     UserEntity? user = userRL.GetUserById(resetToken.UserId);
    //
    //     if (user == null)
    //     {
    //         return new ResponseModel<string>
    //         {
    //             IsSuccess = false,
    //             Message = "User not found.",
    //             Data = null
    //         };
    //     }
    //
    //     var passwordHasher = new PasswordHasher<UserEntity>();
    //
    //     user.Password = passwordHasher.HashPassword(
    //         user,
    //         model.NewPassword
    //     );
    //
    //
    //     userRL.UpdatePassword(user);
    //     userRL.MarkResetTokenAsUsed(resetToken);
    //     
    //     if (model.NewPassword != model.ConfirmPassword)
    //     {
    //         return new ResponseModel<string>
    //         {
    //             IsSuccess = false,
    //             Message = "Passwords do not match.",
    //             Data = null
    //         };
    //     }
    //     
    //     return new ResponseModel<string>
    //     {
    //         IsSuccess = true,
    //         Message = "Your password has been reset.",
    //         Data = null
    //     };

    
    public async Task<ResponseModel<string>> ResetPassword(
        ResetPasswordModel model)
    {
        if (model.NewPassword != model.ConfirmPassword)
        {
            return new ResponseModel<string>
            {
                IsSuccess = false,
                Message = "Passwords do not match.",
                Data = null
            };
        }

        int userId;

        try
        {
            userId = jwtService.ValidatePasswordResetToken(model.Token);
        }
        catch (SecurityTokenExpiredException)
        {
            return new ResponseModel<string>
            {
                IsSuccess = false,
                Message = "Reset token has expired.",
                Data = null
            };
        }
        catch (SecurityTokenException)
        {
            return new ResponseModel<string>
            {
                IsSuccess = false,
                Message = "Invalid reset token.",
                Data = null
            };
        }

        UserEntity? user = userRL.GetUserById(userId);

        if (user == null)
        {
            return new ResponseModel<string>
            {
                IsSuccess = false,
                Message = "User not found.",
                Data = null
            };
        }

        var passwordHasher = new PasswordHasher<UserEntity>();

        user.Password = passwordHasher.HashPassword(
            user,
            model.NewPassword
        );

        userRL.UpdatePassword(user);

        return new ResponseModel<string>
        {
            IsSuccess = true,
            Message = "Your password has been reset successfully.",
            Data = null
        };
    }   
    
    
    public ResponseModel<LoginResponseModel> GoogleLoginBL(
        string email,
        string providerSubject,
        string firstName,
        string lastName)
    {
        try
        {
            UserEntity? user = userRL.FindUserByExternalLogin(
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
            return new ResponseModel<LoginResponseModel>
            {
                IsSuccess = false,
                Message = ex.Message,
                Data = null
            };
        }
    }
}
