using System.ComponentModel.DataAnnotations;  // For data annotations
using System.ComponentModel.DataAnnotations.Schema;  // For custom table or column settings

namespace TimeTracker.Models.Dto{
    public class TimeLogDto{
        public int Id { get; set; }
        public int UserId { get; set; }  // Foreign key to User
        public required string Title { get; set; }  // Optional: Could represent the log title (e.g., "Project A")
        public DateTime CreatedAt { get; set; }
    }
}