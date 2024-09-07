using System.ComponentModel.DataAnnotations;  // For data annotations
using System.ComponentModel.DataAnnotations.Schema;  // For custom table or column settings

namespace TimeTracker.Models
{
    public class User
    {
        [Key]  // This marks 'netID' as the primary key
        public required string NetID { get; set; }

        // [Required]  // Makes 'Email' a required field
        // [EmailAddress]  // Specifies that 'Email' should follow an email format
        // public string Email { get; set; }

        [Required]
        [MaxLength(64)]  // SHA-256 hash is 64 characters long
        public required string Password { get; set; }

        // [Column(TypeName = "datetime2")]  // Specify database type if needed
        public DateTime CreatedAt { get; set; } = DateTime.Now;  // Default value for CreatedAt
    }
}
