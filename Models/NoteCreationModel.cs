using System.ComponentModel.DataAnnotations;

namespace Models;

public class NoteCreationModel
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; }
    [Required]
    public string Description { get; set; }
    [Required]
    [MaxLength(30)]
    public string Colour { get; set; }
}