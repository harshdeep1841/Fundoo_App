using BusinessLayer.Interface;
using MediatR;
using Models;

namespace BusinessLayer.Commands.Users;

public class ResetPasswordHandler
    : IRequestHandler<ResetPasswordCommand, ResponseModel<string>>
{
    private readonly IUserBL userBL;

    public ResetPasswordHandler(IUserBL userBL)
    {
        this.userBL = userBL;
    }

    public Task<ResponseModel<string>> Handle(
        ResetPasswordCommand request,
        CancellationToken cancellationToken)
    {
        return userBL.ResetPassword(
            request.ResetPasswordModel);
    }
}