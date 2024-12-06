// *************************************************
// **************** Written by: ********************
// ************** Aayush P. and Chris N. ***********
// *************************************************

using Microsoft.AspNetCore.Authorization; // Provides attributes for securing endpoints
using Microsoft.AspNetCore.Mvc; // Enables building RESTful APIs
using TimeTracker.Models.Dto; // Contains Data Transfer Objects (DTOs) for TimeLog and TimeLogEntry
using TimeTracker.Services; // Includes services for handling business logic related to time logs and authentication
using TimeTracker.Extensions; // Contains custom extension methods, such as extracting the NetID from user claims

namespace TimeTracker.Controllers
{
    [ApiController] // Identifies this class as a REST API controller, enabling features like automatic model validation
    [Route("api/timelogs")] // Specifies the base route for all endpoints in this controller
    public class TimeLogController : ControllerBase
    {
        private readonly TimeLogService _timeLogService; // Service to handle operations related to time logs
        private readonly AuthService _authService; // Service to manage user authentication and authorization

        // Constructor for injecting dependencies
        // Both TimeLogService and AuthService are injected at runtime via dependency injection
        public TimeLogController(TimeLogService timeLogService, AuthService authService) // Written by: Chris N.
        {
            _timeLogService = timeLogService; // Assigns the injected TimeLogService instance to the private field
            _authService = authService; // Assigns the injected AuthService instance to the private field
        }

        // GET: api/timelogs/me
        // Retrieves all time logs for the currently authenticated user
        [HttpGet("me")]
        [Authorize] // Ensures only authenticated users can access this endpoint
        public async Task<IActionResult> GetTimeLogs() // Written by: Chris N.
        {
            // Extracts the NetID of the currently logged-in user from their JWT claims
            var userNetId = User.GetUserNetId();

            // Retrieves the user object using the extracted NetID
            var user = _authService.GetByNetId(userNetId);
            if (user == null) // If no user is found, return an Unauthorized response
            {
                return Unauthorized("User not found or not authorized.");
            }

            // Fetches all time logs associated with the user's ID
            var timeLogs = await _timeLogService.GetTimeLogsForUser(user.Id);

            // Returns the retrieved time logs in the HTTP response with a 200 status code
            return Ok(timeLogs);
        }

        // GET: api/timelogs/current
        // Retrieves the time log for the current week for the logged-in user
        [HttpGet("current")]
        [Authorize]
        public async Task<IActionResult> GetCurrentTimeLog() // Written by: Chris N.
        {
            // Extracts the NetID of the currently logged-in user
            var userNetId = User.GetUserNetId();

            // Retrieves the user object using the NetID
            var user = _authService.GetByNetId(userNetId);
            if (user == null) // If the user doesn't exist, return an Unauthorized response
            {
                return Unauthorized("User not found or not authorized.");
            }

            // Retrieves or creates a time log for the current week for the user
            var timeLog = await _timeLogService.GetOrCreateTimeLogForCurrentWeek(user.Id);

            // Returns the current week's time log with a 200 status code
            return Ok(timeLog);
        }

        // GET: api/timelogs/{id}
        // Retrieves a specific time log by its ID
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetTimeLog(int id) // Written by: Chris N.
        {
            // Extracts the NetID of the currently logged-in user
            var userNetId = User.GetUserNetId();

            // Retrieves the user object using the NetID
            var user = _authService.GetByNetId(userNetId);
            if (user == null) // If the user doesn't exist, return an Unauthorized response
            {
                return Unauthorized("User not found or not authorized.");
            }

            // Fetches the time log using its ID
            var timeLog = await _timeLogService.GetTimeLog(id);

            // Validates that the time log exists and belongs to the authenticated user
            if (timeLog == null || timeLog.UserId != user.Id)
            {
                return NotFound("Time log not found."); // Returns a 404 status code if the time log is invalid
            }

            // Returns the time log with a 200 status code
            return Ok(timeLog);
        }

        // POST: api/timelogs/entry
        // Adds a new time log entry for the current week for the logged-in user
        [HttpPost("entry")]
        [Authorize]
        public async Task<IActionResult> AddTimeLogEntry([FromForm] TimeLogEntryDto entryDto) // Written by: Aayush P.
        {
            // Validates the incoming model (entryDto)
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Returns a 400 status code if the model is invalid
            }

            // Extracts the NetID of the currently logged-in user
            var userNetId = User.GetUserNetId();

            // Retrieves the user object using the NetID
            var user = _authService.GetByNetId(userNetId);
            if (user == null) // If the user doesn't exist, return an Unauthorized response
            {
                return Unauthorized("User not found or not authorized.");
            }

            // Adds the time log entry to the current week's time log
            await _timeLogService.AddTimeLogEntryForCurrentWeek(user.Id, entryDto.Duration, entryDto.Description);

            // Returns a success message with a 200 status code
            return Ok("Time log entry added for the current week.");
        }

        // GET: api/timelogs/{timeLogId}/entries
        // Retrieves all time log entries for a specific time log
        [HttpGet("{timeLogId}/entries")]
        [Authorize]
        public async Task<IActionResult> GetTimeLogEntries(int timeLogId) // Written by: Aayush P.
        {
            // Extracts the NetID of the currently logged-in user
            var userNetId = User.GetUserNetId();

            // Retrieves all entries associated with the specified time log
            var timeLogEntries = await _timeLogService.GetTimeLogEntries(timeLogId);

            // Returns the retrieved entries with a 200 status code
            return Ok(timeLogEntries);
        }

        // PATCH: api/timelogs/entry/{id}
        // Updates a specific time log entry for the logged-in user
        [HttpPatch("entry/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateTimeLogEntry(int id, [FromForm] TimeLogEntryDto entryDto) // Written by: Aayush P.
        {
            // Validates the incoming model (entryDto)
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Returns a 400 status code if the model is invalid
            }

            // Extracts the NetID of the currently logged-in user
            var userNetId = User.GetUserNetId();

            // Retrieves the user object using the NetID
            var user = _authService.GetByNetId(userNetId);
            if (user == null) // If the user doesn't exist, return an Unauthorized response
            {
                return Unauthorized("User not found or not authorized.");
            }

            // Fetches the time log entry by its ID
            var timeLogEntry = await _timeLogService.GetTimeLogEntry(id);

            // Validates that the time log entry exists and belongs to the authenticated user
            if (timeLogEntry == null || timeLogEntry.TimeLog.UserId != user.Id)
            {
                return NotFound("Time log entry not found."); // Returns a 404 status code if the entry is invalid
            }

            // Updates the time log entry with the new data
            await _timeLogService.UpdateTimeLogEntry(id, entryDto.Duration, entryDto.Description);

            // Returns a success message with a 200 status code
            return Ok("Time log entry updated.");
        }

        // DELETE: api/timelogs/entry/{id}
        // Deletes a specific time log entry for the logged-in user
        [HttpDelete("entry/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteTimeLogEntry(int id) // Written by: Aayush P.
        {
            // Extracts the NetID of the currently logged-in user
            var userNetId = User.GetUserNetId();

            // Retrieves the user object using the NetID
            var user = _authService.GetByNetId(userNetId);
            if (user == null) // If the user doesn't exist, return an Unauthorized response
            {
                return Unauthorized("User not found or not authorized.");
            }

            // Fetches the time log entry by its ID
            var timeLogEntry = await _timeLogService.GetTimeLogEntry(id);

            // Validates that the time log entry exists and belongs to the authenticated user
            if (timeLogEntry == null || timeLogEntry.TimeLog.UserId != user.Id)
            {
                return NotFound("Time log entry not found."); // Returns a 404 status code if the entry is invalid
            }

            // Deletes the specified time log entry
            await _timeLogService.DeleteTimeLogEntry(id);

            // Returns a success message with a 200 status code
            return Ok("Time log entry deleted.");
        }
    }
}