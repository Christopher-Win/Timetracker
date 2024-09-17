using System.ComponentModel.DataAnnotations;  // For data annotations
using System.ComponentModel.DataAnnotations.Schema;  // For custom table or column settings

namespace TimeTracker.Models.Dto{
    public class TimeLogEntryDto
    {
        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        [Required]
        public string Description { get; set; }
        public int Duration { get; internal set; } 
    }
}