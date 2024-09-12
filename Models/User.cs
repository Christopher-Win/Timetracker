using System.ComponentModel.DataAnnotations;  // For data annotations
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;  // For custom table or column settings

namespace TimeTracker.Models
{
    [Index(nameof(NetID), IsUnique = true)]  // Makes 'NetID' a required and unique field
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public required string NetID { get; set; }

        [Required]
        [MaxLength(64)]  // SHA-256 hash is 64 characters long
        public required string Password { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;  // Default value for CreatedAt
        public List<TimeLog>? TimeLogs { get; set; }

    }
}
