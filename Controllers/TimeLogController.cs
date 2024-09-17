using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Models;
using TimeTracker.Models.Dto;
using TimeTracker.Services;
using TimeTracker.Extensions;

namespace TimeTracker.Controllers{

    [ApiController]
    [Route("api/timelogs")]
    public class TimeLogController : ControllerBase
    {
        private readonly TimeLogService _timeLogService;
        private readonly AuthService _authService;
        public TimeLogController(TimeLogService timeLogService, AuthService authService)
        {
            _timeLogService = timeLogService;
            _authService = authService;
        }

        // GET: api/timelog -> returns all time logs for the logged in user.
        [HttpGet("")]
        [Authorize]
        public async Task<IActionResult> GetTimeLogs()
        {
            var userNetId = User.GetUserNetId();
            int userId = _authService.GetByNetId(userNetId).Id;
            var timeLogs = await _timeLogService.GetTimeLogsForUser(userId);
            return Ok(timeLogs);
        }

        // GET: api/timelog/current -> returns the time log for the current week of the logged in user.
        [HttpGet("current")]
        [Authorize]
        public async Task<IActionResult> GetCurrentTimeLog()
        {
            var userNetId = User.GetUserNetId();
            int userId = _authService.GetByNetId(userNetId).Id;
            var timeLog = await _timeLogService.GetOrCreateTimeLogForCurrentWeek(userId);
            return Ok(timeLog);
        }
        
        // GET: api/timelog/{id} -> returns the time log with the specified id.
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetTimeLog(int id)
        {
            var userNetId = User.GetUserNetId();
            int userId = _authService.GetByNetId(userNetId).Id;
            var timeLog = await _timeLogService.GetTimeLog(id); // Makes sure a user can only access their time logs.
            if (timeLog == null || timeLog.UserId != userId)
            {
                return NotFound("Time log not found.");
            }
            return Ok(timeLog);
        }
        
        // POST: api/timelog/entry -> adds a time log entry for the current week of the logged in user.
        [HttpPost("entry")]
        [Authorize]
        public async Task<IActionResult> AddTimeLogEntry([FromForm] TimeLogEntryDto entryDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userNetId = User.GetUserNetId();
            int userId = _authService.GetByNetId(userNetId).Id;

            await _timeLogService.AddTimeLogEntryForCurrentWeek(userId, entryDto.StartTime, entryDto.EndTime, entryDto.Description);
            return Ok("Time log entry added for the current week.");
        }

        
        // GET: /api/timelog/{timeLogId}/entries -> returns all time log entries for the specified time log.
        [HttpGet("{timeLogId}/entries")]
        [Authorize]
        public async Task<IActionResult> GetTimeLogEntries(int timeLogId)
        {
            var userNetId = User.GetUserNetId();
            
            var timeLogEntries = await _timeLogService.GetTimeLogEntries(timeLogId);
            // Console.WriteLine("Data requested by " + user.NetID);
            return Ok(timeLogEntries);
        }

    }
}