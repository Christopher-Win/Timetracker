using Microsoft.AspNetCore.Mvc;
using TimeTracker.Models;
using TimeTracker.Models.Dto;
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
        public async Task<IActionResult> AddTimeLogEntry([FromForm] TimeLogEntryDto entryDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            await _timeLogService.AddTimeLogEntryForCurrentWeek(entryDto.userId, entryDto.StartTime, entryDto.EndTime, entryDto.Description);
            return Ok("Time log entry added for the current week.");
        }

        // GET: api/timelog
        [HttpGet("")]
        public async Task<IActionResult> GetTimeLog()
        {
            var timeLog = await _timeLogService.GetOrCreateTimeLogForCurrentWeek(1);
            return Ok(timeLog);
        }
    }
}