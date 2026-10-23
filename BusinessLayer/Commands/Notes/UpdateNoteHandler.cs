using BusinessLayer.Interface;
using BusinessLayer.Service;
using MediatR;
using Models;
using RepositoryLayer.Entity;

namespace BusinessLayer.Commands.Notes;

public class UpdateNoteHandler : IRequestHandler<UpdateNoteCommand , ResponseModel<NoteEntity>>
{
    public INotesBL notesBL;

    public UpdateNoteHandler(INotesBL notesBL)
    {
        this.notesBL = notesBL;
    }
    public Task<ResponseModel<NoteEntity>> Handle(UpdateNoteCommand request, CancellationToken cancellationToken)
    {
        return notesBL.EditNote(request.Id, request.Note , request.UserId);
    }
}