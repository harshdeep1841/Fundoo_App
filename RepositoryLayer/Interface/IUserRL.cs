using Models;
using RepositoryLayer.Entity;

namespace RepositoryLayer.Interface;

public interface IUserRL
{
    public ResponseModel<RegistrationModel> RegisterUserRL(RegistrationModel register);
    public UserEntity? LoginUserRL(LoginModel login);
    public UserEntity? FindUserByEmail(string email);
    public void SaveResetToken(PasswordResetTokenEntity token);
    public PasswordResetTokenEntity? TokenExists(string token);

    public UserEntity? GetUserById(int userId);
    public void UpdatePassword(UserEntity user);

    public void MarkResetTokenAsUsed(
        PasswordResetTokenEntity resetToken);
}