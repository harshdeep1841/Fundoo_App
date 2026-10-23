using BusinessLayer.Interface;
using BusinessLayer.Service;
using MediatR;
using Models;
using RepositoryLayer.Entity;

namespace BusinessLayer.Queries.Notes;

public class GetNotesHandler : IRequestHandler<GetNotesQuery , ResponseModel<List<NoteEntity>>>
{
    public INotesBL notesBL;
    public GetNotesHandler(INotesBL notesBL)
    {
        this.notesBL = notesBL;
    }
    public Task<ResponseModel<List<NoteEntity>>> Handle(GetNotesQuery request, CancellationToken cancellationToken)
    {
        return notesBL.ViewNotes(request.UserId);
    }
}