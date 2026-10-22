using BusinessLayer.Service.PasswordHandler;
using MediatR;
using Models;
using RepositoryLayer.Entity;
using RepositoryLayer.Interface;

namespace BusinessLayer.Commands.Users;

public class RegisterUserHandler
    : IRequestHandler<RegisterUserCommand, ResponseModel<string>>
{
    private readonly IUserRL userRL;
    private readonly PasswordService _passwordService;

    public RegisterUserHandler(
        IUserRL userRL,
        PasswordService passwordService)
    {
        this.userRL = userRL;
        _passwordService = passwordService;
    }

    public Task<ResponseModel<string>> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        var model = request.RegisterModel;

        var (hash, salt) =
            _passwordService.HashPassword(model.Password);

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

        var response = new ResponseModel<string>
        {
            IsSuccess = true,
            Message = "Registration successful.",
            Data = user.Email
        };

        return Task.FromResult(response);
    }
}