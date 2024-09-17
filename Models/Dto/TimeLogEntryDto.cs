using System.ComponentModel.DataAnnotations;  // For data annotations
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;  // For custom table or column settings

namespace TimeTracker.Models.Dto{
    public class TimeLogEntryDto
    {
        [BindNever] // Prevents the Id property from being required in request body
        public int Id { get; set; }
        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }
        public DateTime CreatedAt { get; internal set; }
        [Required]
        public string Description { get; set; }
        public int Duration { get; internal set; } 
    }
}