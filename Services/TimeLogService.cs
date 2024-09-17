using Microsoft.EntityFrameworkCore;
using TimeTracker.Data;
using TimeTracker.Models;
using TimeTracker.Models.Dto;
using System.Security.Claims;

using static TimeTracker.Services.AuthService;
namespace TimeTracker.Services{
    public class TimeLogService
    {
        private readonly ApplicationDBContext _context;

        public TimeLogService(ApplicationDBContext context)
        {
            _context = context;
        }

        // Helper method to calculate the start of the week (e.g., Sunday)
        private DateTime GetStartOfWeek(DateTime date)
        {
            var diff = date.DayOfWeek - DayOfWeek.Sunday;
            if (diff < 0)
                diff += 7;
            return date.AddDays(-1 * diff).Date;
        }

        // Method to get all TimeLogs for a specific user
        public async Task<List<TimeLogDto>> GetTimeLogsForUser(int userId)
        {
            var timeLogs = await _context.TimeLogs
                .Where(t => t.UserId == userId)
                .ToListAsync();

            var timeLogDtos = new List<TimeLogDto>(); // Create a list to store TimeLogDto objects

            foreach (var timeLog in timeLogs)
            {
                var timeLogDto = new TimeLogDto
                {
                    Id = timeLog.Id,
                    UserId = timeLog.UserId,
                    Title = timeLog.Title,
                    CreatedAt = timeLog.CreatedAt
                };
                timeLogDtos.Add(timeLogDto);
            }

            return timeLogDtos; // Return a list of TimeLogDto objects
        }
        // Method to get a specific TimeLog for a user
        public async Task<TimeLogDto> GetTimeLog(int timeLogId)
        {
            var timeLog = await _context.TimeLogs
                .SingleOrDefaultAsync(t => t.Id == timeLogId);

            if (timeLog == null)
            {
                return null;
            }

            var timeLogDto = new TimeLogDto
            {
                Id = timeLog.Id,
                UserId = timeLog.UserId,
                Title = timeLog.Title,
                CreatedAt = timeLog.CreatedAt
            };

            return timeLogDto;
        }
        // Method to get or create the TimeLog for the current week
        public async Task<TimeLogDto> GetOrCreateTimeLogForCurrentWeek(int userId)
        {
            var startOfWeek = GetStartOfWeek(DateTime.Now);
            var existingTimeLog = await _context.TimeLogs.Include(t => t.TimeLogEntries)
                .SingleOrDefaultAsync(t => t.UserId == userId && t.CreatedAt >= startOfWeek);

            if (existingTimeLog != null)
            {
                var timeLogDto = new TimeLogDto
                {
                    Id = existingTimeLog.Id,
                    UserId = existingTimeLog.UserId,
                    Title = existingTimeLog.Title,
                    CreatedAt = existingTimeLog.CreatedAt
                };
                return timeLogDto;
            }

            // Create a new TimeLog for the current week if it doesn't exist
            var newTimeLog = new TimeLog
            {
                UserId = userId,
                Title = $"Weekly Log for {startOfWeek:MMMM dd, yyyy}",
                CreatedAt = DateTime.Now
            };

            _context.TimeLogs.Add(newTimeLog);
            await _context.SaveChangesAsync();

            return new TimeLogDto
                {
                    Id = newTimeLog.Id,
                    UserId = newTimeLog.UserId,
                    Title = newTimeLog.Title,
                    CreatedAt = newTimeLog.CreatedAt
                };
        }

        // Method to add a TimeLogEntry to the current week's TimeLog for the user
        public async Task AddTimeLogEntryForCurrentWeek(int userId, DateTime startTime, DateTime endTime, string description)
        {
            var currentWeekLog = await GetOrCreateTimeLogForCurrentWeek(userId); // Get the User's TimeLog for the current week given their userId
            
            var timeLogEntry = new TimeLogEntry
            {
                TimeLogId = currentWeekLog.Id, 
                StartTime = startTime,
                EndTime = endTime,
                Duration = (int)(endTime - startTime).TotalMinutes,
                Description = description,
                CreatedAt = DateTime.Now
            };

            _context.TimeLogEntries.Add(timeLogEntry);
            await _context.SaveChangesAsync();
        }

        // Method to get all time entries for a specific TimeLog
        public async Task<List<TimeLogEntryDto>> GetTimeLogEntries(int timeLogId)
        {
            var timeEntries = await _context.TimeLogEntries
            .Where(t => t.TimeLogId == timeLogId)
            .ToListAsync();

            var timeLogEntryDtos = new List<TimeLogEntryDto>(); // Create a list to store TimeLogEntryDto objects

            foreach (var entry in timeEntries)
            {
                var timeLogEntryDto = new TimeLogEntryDto
                {
                    StartTime = entry.StartTime,
                    EndTime = entry.EndTime,
                    Duration = (int)(entry.EndTime - entry.StartTime).TotalMinutes,
                    Description = entry.Description
                };
            timeLogEntryDtos.Add(timeLogEntryDto);
            }

            return timeLogEntryDtos; // Return a list of TimeLogEntryDto objects
        }

    }
}