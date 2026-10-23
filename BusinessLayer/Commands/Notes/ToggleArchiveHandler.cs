using BusinessLayer.Interface;
using BusinessLayer.Service;
using MediatR;
using Models;

namespace BusinessLayer.Commands.Notes;

public class ToggleArchiveHandler : IRequestHandler<ToggleArchiveCommand, ResponseModel<bool>>
{
    public INotesBL notesBL;

    public ToggleArchiveHandler(INotesBL notesBL)
    {
        this.notesBL = notesBL;
    }
    public Task<ResponseModel<bool>> Handle(ToggleArchiveCommand request, CancellationToken cancellationToken)
    {
        return notesBL.ToggleTrash(request.UserId, request.Id);
    }
}