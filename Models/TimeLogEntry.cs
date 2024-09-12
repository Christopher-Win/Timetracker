using System.ComponentModel.DataAnnotations;  // For data annotations
using System.ComponentModel.DataAnnotations.Schema;  // For custom table or column settings

namespace TimeTracker.Models
{
   public class TimeLogEntry
{
    public int Id { get; set; }
    public int UserId { get; set; }  // Id of the user who created the entry
    public int TimeLogId { get; set; }  // Foreign key to TimeLog
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int Duration { get; set; }  // Duration in minutes
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public TimeLog TimeLog { get; set; }  // Navigation property to TimeLog
    }
}
