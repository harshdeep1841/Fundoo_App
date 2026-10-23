using BusinessLayer.Interface;
using MediatR;
using Models;

namespace BusinessLayer.Commands.Users;

public class ForgetPasswordHandler
    : IRequestHandler<ForgetPasswordCommand, ResponseModel<string>>
{
    private readonly IUserBL userBL;

    public ForgetPasswordHandler(IUserBL userBL)
    {
        this.userBL = userBL;
    }

    public Task<ResponseModel<string>> Handle(
        ForgetPasswordCommand request,
        CancellationToken cancellationToken)
    {
        return userBL.ForgetPasswordBL(
            request.ForgetPasswordModel);
    }
}