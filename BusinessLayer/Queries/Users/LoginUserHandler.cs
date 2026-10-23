using BusinessLayer.Interface;
using MediatR;
using Models;

namespace BusinessLayer.Queries.Users;

public class LoginUserHandler
    : IRequestHandler<LoginUserQuery,
        ResponseModel<LoginResponseModel>>
{
    private readonly IUserBL userBL;

    public LoginUserHandler(IUserBL userBL)
    {
        this.userBL = userBL;
    }

    public Task<ResponseModel<LoginResponseModel>> Handle(
        LoginUserQuery request,
        CancellationToken cancellationToken)
    {
        var response = userBL.LoginUserBL(request.LoginModel);

        return Task.FromResult(response);
    }
}