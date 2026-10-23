using MediatR;
using Models;

namespace BusinessLayer.Commands.Notes;

public class ToggleArchiveCommand : IRequest<ResponseModel<bool>>
{
    public int Id { get; set; }
    public int UserId { get; set; }
}