using System.Security.Cryptography;
using System.Text;
using BusinessLayer.Interface;
using Microsoft.AspNetCore.Identity;
using Models;
using RepositoryLayer.Entity;
using RepositoryLayer.Interface;

namespace BusinessLayer.Service;

public class UserBL : IUserBL
{
    public IUserRL userRL;
    public JwtService jwtService;
    public EmailClient client;
    public UserBL(IUserRL userRL , JwtService jwtService , EmailClient client)
    {
        this.userRL = userRL;
        this.jwtService = jwtService;
        this.client = client;
    }
    
    public ResponseModel<RegistrationModel> RegisterUserBL(RegistrationModel register)
    {
        return userRL.RegisterUserRL(register);
    }

    public ResponseModel<LoginResponseModel> LoginUserBL(LoginModel login)
    {
       
        
        UserEntity? userEntity =   userRL.LoginUserRL(login);
        ResponseModel<LoginResponseModel> rm = new ResponseModel<LoginResponseModel>();
        if (userEntity == null)
        {
            rm.IsSuccess = false;
            rm.Message = "login email or password is wrong";
            rm.Data = null;
        }
        else
        {
            rm.IsSuccess = true;
            rm.Message = "login success";
            rm.Data = new LoginResponseModel
            {
                Email = userEntity.Email,
                Token = jwtService.GenerateToken(userEntity)
            };
        }
        return rm;
    }

    public async  Task<ResponseModel<string>> ForgetPasswordBL(ForgetPasswordModel model)
    {
      
        UserEntity? userEntity =   userRL.FindUserByEmail(model.Email);
        if (userEntity == null)
        {
            return null;
        }
        
        
        
        
        var tokenBytes = RandomNumberGenerator.GetBytes(32);
        string token = Convert.ToHexString(tokenBytes);
        DateTime expireAt = DateTime.UtcNow.AddMinutes(15);
        
        string tokenHash = Convert.ToHexString(
            SHA256.HashData(
                Encoding.UTF8.GetBytes(token)
            )
        );

        PasswordResetTokenEntity passwordResetTokenEntity = new PasswordResetTokenEntity()
        {
            UserId = userEntity.UserId,
            ExpiresAt = expireAt,
            TokenHash =  tokenHash,
            User =  userEntity
                
        };
         userRL.SaveResetToken(passwordResetTokenEntity);

         
         var email = new EmailModel.Model.EmailModel
         {
             To = userEntity.Email,
             Subject = "Fundoo Password Reset",
             Body = $"""
                     Your password reset token is:
                     {token}
                     This token will expire in 15 minutes.
                     """
         };
        await client.SendEmail(email);
        return new ResponseModel<string>
        {
            IsSuccess = true,
            Message = "If an account exists, a password reset link has been sent.",
            Data = null
        };

    }


    public async Task<ResponseModel<string>> ResetPassword(ResetPasswordModel model)
    {
        string tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(model.Token)));
        PasswordResetTokenEntity? resetToken =
           userRL.TokenExists(tokenHash);

        if (resetToken == null)
        {
            return new ResponseModel<string>
            {
                IsSuccess = false,
                Message = "Invalid reset token.",
                Data = null
            };
        }

        if (resetToken.IsUsed)
        {
            return new ResponseModel<string>
            {
                IsSuccess = false,
                Message = "Reset token has already been used.",
                Data = null
            };
        }

        if (resetToken.ExpiresAt < DateTime.UtcNow)
        {
            return new ResponseModel<string>
            {
                IsSuccess = false,
                Message = "Reset token has expired.",
                Data = null
            };
        }
        
        
        
        UserEntity? user = userRL.GetUserById(resetToken.UserId);

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
        userRL.MarkResetTokenAsUsed(resetToken);
        
        if (model.NewPassword != model.ConfirmPassword)
        {
            return new ResponseModel<string>
            {
                IsSuccess = false,
                Message = "Passwords do not match.",
                Data = null
            };
        }
        
        return new ResponseModel<string>
        {
            IsSuccess = true,
            Message = "Your password has been reset.",
            Data = null
        };
    }
}