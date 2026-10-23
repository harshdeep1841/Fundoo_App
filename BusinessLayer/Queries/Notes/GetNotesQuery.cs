using MediatR;
using Models;
using RepositoryLayer.Entity;

namespace BusinessLayer.Queries.Notes;

public class GetNotesQuery : IRequest<ResponseModel<List<NoteEntity>>>
{
    public int UserId { get; set; }
}