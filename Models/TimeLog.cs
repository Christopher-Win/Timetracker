// *************************************************
// ***************** Written by: *******************
// ****************** Chris N. *********************
// *************************************************

using System.ComponentModel.DataAnnotations;  // For data annotations
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;  // For custom table or column settings

namespace TimeTracker.Models
{
    public class TimeLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }  // Foreign key to User
        public required string Title { get; set; }  // Optional: Could represent the log title (e.g., "Project A")
        public DateTime CreatedAt { get; set; }
        [JsonIgnore]//telling the serializer to ignore the TimeLog navigation property when serializing the TimeLogEntry object, which breaks the circular reference.
        public  User User { get; set; }  // Navigation property to User
        public List<TimeLogEntry>? TimeLogEntries { get; set; }  // Navigation property to TimeLogEntries
    }
}