using System.ComponentModel.DataAnnotations;  // For data annotations
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;  // For custom table or column settings
using System.Text.Json.Serialization;

namespace TimeTracker.Models
{
    [Index(nameof(NetID), IsUnique = true)]  // Makes 'NetID' a required and unique field
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // 
        public int Id { get; set; }

        [Required]
        public required string NetID { get; set; }
        public string Role { get; set; } = "Student";
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [Required]
        [MaxLength(64)]  // SHA-256 hash is 64 characters long
        [JsonIgnore]
        public string Password { get; set; } // Default password UTDID
        public bool IsDefaultPassword { get; set; } = true; // Default to true for new users
        public DateTime CreatedAt { get; set; } = DateTime.Now;  // Default value for CreatedAt
        public List<TimeLog>? TimeLogs { get; set; }
    }
}
