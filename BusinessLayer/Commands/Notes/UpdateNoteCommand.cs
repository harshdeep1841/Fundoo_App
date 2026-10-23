using MediatR;
using Models;
using RepositoryLayer.Entity;

namespace BusinessLayer.Commands.Notes;

public class UpdateNoteCommand :IRequest<ResponseModel<NoteEntity>>
{
    public int Id { get; set; }
    public EditNoteModel Note { get; set; }
    public int UserId { get; set; }
}