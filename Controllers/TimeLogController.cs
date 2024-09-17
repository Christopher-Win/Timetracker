using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Models;
using TimeTracker.Models.Dto;
using TimeTracker.Services;
using static TimeTracker.Controllers.AuthController;

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
        private int GetCurrentUserId() // Helper method to get the current user's ID
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException("User ID claim not found.");
            }
            var user = _authService.GetById(userIdClaim);
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found.");
            }
            return user.Id;
        }

        // GET: api/timelog -> returns all time logs for the logged in user.
        [HttpGet("")]
        [Authorize]
        public async Task<IActionResult> GetTimeLogs()
        {
            var userId = GetCurrentUserId();
            var timeLogs = await _timeLogService.GetTimeLogsForUser(userId);
            return Ok(timeLogs);
        }

        // GET: api/timelog/current -> returns the time log for the current week of the logged in user.
        [HttpGet("current")]
        [Authorize]
        public async Task<IActionResult> GetCurrentTimeLog()
        {
            var userId = GetCurrentUserId();
            var timeLog = await _timeLogService.GetOrCreateTimeLogForCurrentWeek(userId);
            return Ok(timeLog);
        }
        
        // GET: api/timelog/{id} -> returns the time log with the specified id.
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetTimeLog(int id)
        {
            var userId = GetCurrentUserId();
            var timeLog = await _timeLogService.GetTimeLog(id);
            if (timeLog == null)
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
            var netIDClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = _authService.GetById(netIDClaim);

            await _timeLogService.AddTimeLogEntryForCurrentWeek(user.Id, entryDto.StartTime, entryDto.EndTime, entryDto.Description);
            return Ok("Time log entry added for the current week.");
        }

        
        // GET: /api/timelog/{timeLogId}/entries -> returns all time log entries for the specified time log.
        [HttpGet("{timeLogId}/entries")]
        [Authorize]
        public async Task<IActionResult> GetTimeLogEntries(int timeLogId)
        {
            var userId = GetCurrentUserId();
            
            var timeLogEntries = await _timeLogService.GetTimeLogEntries(timeLogId);
            // Console.WriteLine("Data requested by " + user.NetID);
            return Ok(timeLogEntries);
        }

    }
}