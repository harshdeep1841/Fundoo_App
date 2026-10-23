using BusinessLayer.Interface;
using MediatR;
using Models;

namespace BusinessLayer.Commands.Notes;

public class DeleteNotesHandler : IRequestHandler<DeleteNotesCommand , ResponseModel<bool>>
{
    public INotesBL notesBL;

    public DeleteNotesHandler(INotesBL notesBL)
    {
        this.notesBL = notesBL;
    }
    public Task<ResponseModel<bool>> Handle(DeleteNotesCommand request, CancellationToken cancellationToken)
    {
        return notesBL.DeleteNote(request.Id , request.UserId);
    }
}