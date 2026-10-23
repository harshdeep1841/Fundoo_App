using BusinessLayer.Interface;
using MediatR;
using Models;

namespace BusinessLayer.Commands.Notes;

public class ToggleTrashHandler : IRequestHandler<ToggleTrashCommand , ResponseModel<bool>>
{
    private INotesBL notesBL;

    public ToggleTrashHandler(INotesBL notesBL)
    {
        this.notesBL = notesBL;
    }
    public Task<ResponseModel<bool>> Handle(ToggleTrashCommand request, CancellationToken cancellationToken)
    {
        return notesBL.ToggleTrash(request.UserId, request.Id);
    }
}