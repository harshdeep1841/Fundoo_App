using MediatR;
using Models;

namespace BusinessLayer.Commands.Notes;

public class AddNotesCommand : IRequest<ResponseModel<string>>
{
    public NoteCreationModel Note { get; set; }  = new NoteCreationModel();
    public int UserId { get; set; }
}