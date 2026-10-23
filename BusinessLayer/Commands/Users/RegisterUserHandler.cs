using BusinessLayer.Interface;
using MediatR;
using Models;

namespace BusinessLayer.Commands.Users;

public class RegisterUserHandler
    : IRequestHandler<RegisterUserCommand, ResponseModel<string>>
{
    private readonly IUserBL userBL;

    public RegisterUserHandler(IUserBL userBL)
    {
        this.userBL = userBL;
    }

    public Task<ResponseModel<string>> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        return userBL.RegisterUserBL(request.RegisterModel);
    }
}