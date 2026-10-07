using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RepositoryLayer.Entity;

public class PasswordResetTokenEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public int UserId { get; set; }
    public string TokenHash { get; set; } 
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public UserEntity User { get; set; }
    
}