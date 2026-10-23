using BusinessLayer.Interface;
using MediatR;
using Models;

namespace BusinessLayer.Commands.Notes;

public class AddNotesHandler : IRequestHandler<AddNotesCommand , ResponseModel<string>>
{
    public INotesBL notesBL;

    public AddNotesHandler(INotesBL notesBL)
    {
        this.notesBL = notesBL;
    }
    public Task<ResponseModel<string>> Handle(AddNotesCommand request, CancellationToken cancellationToken)
    {
        return notesBL.AddNotes(request.Note, request.UserId);
    }
    
    
}