using Microsoft.AspNetCore.Mvc;
using TimeTracker.Models;
using TimeTracker.Services;
namespace TimeTracker.Controllers{
    
    [ApiController]
    [Route("api/[controller]")]
    public class TimeLogController : ControllerBase
    {
        private readonly TimeLogService _timeLogService;

        public TimeLogController(TimeLogService timeLogService)
        {
            _timeLogService = timeLogService;
        }

        // POST: api/timelog/entry
        [HttpPost("entry")]
        public async Task<IActionResult> AddTimeLogEntry([FromBody] TimeLogEntry entryDto)
        {
            await _timeLogService.AddTimeLogEntryForCurrentWeek(entryDto.UserId, entryDto.StartTime, entryDto.EndTime, entryDto.Description);
            return Ok("Time log entry added for the current week.");
        }
    }
}