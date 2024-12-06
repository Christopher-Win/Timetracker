using Microsoft.EntityFrameworkCore; // Provides ORM functionality for querying and interacting with the database
using TimeTracker.Data; // References the application's database context
using TimeTracker.Models; // References models like TimeLog and TimeLogEntry
using TimeTracker.Models.Dto; // References DTOs for data transfer between layers
using System.Security.Claims; // Enables claims-based identity for user context

using static TimeTracker.Services.AuthService; // Allows reuse of static methods from AuthService

namespace TimeTracker.Services
{
    // Service class for managing TimeLogs and related operations
    public class TimeLogService
    {
        private readonly ApplicationDBContext _context; // Database context for accessing and managing data

        // Constructor to inject the database context
        // Dependency injection ensures a single shared instance of the database context is used.
        public TimeLogService(ApplicationDBContext context)
        {
            _context = context; // Assigns the injected database context to a private field
        }

        // Helper method to calculate the start of the week (e.g., Sunday)
        // This ensures all weekly calculations align with the start of the week.
        private DateTime GetStartOfWeek(DateTime date)
        {
            var diff = date.DayOfWeek - DayOfWeek.Sunday; // Calculates the difference between the current day and Sunday
            if (diff < 0)
                diff += 7; // Adjusts for weeks that cross month boundaries
            return date.AddDays(-1 * diff).Date; // Returns the starting date of the week
        }

        // Retrieves all TimeLogs for a specific user by their User ID
        public async Task<List<TimeLogDto>> GetTimeLogsForUser(int userId)
        {
            // Fetches all TimeLogs associated with the user ID from the database
            var timeLogs = await _context.TimeLogs
                .Where(t => t.UserId == userId) // Filters TimeLogs by UserId
                .ToListAsync(); // Executes the query asynchronously and returns a list

            var timeLogDtos = new List<TimeLogDto>(); // Prepares a list to store TimeLogDto objects

            // Iterates over each TimeLog and maps it to a DTO
            foreach (var timeLog in timeLogs)
            {
                var timeLogDto = new TimeLogDto
                {
                    Id = timeLog.Id,
                    UserId = timeLog.UserId,
                    Title = timeLog.Title,
                    CreatedAt = timeLog.CreatedAt,
                    TimeLogEntries = await GetTimeLogEntries(timeLog.Id) // Fetches entries for the TimeLog
                };
                timeLogDtos.Add(timeLogDto); // Adds the mapped DTO to the list
            }

            return timeLogDtos; // Returns the list of TimeLogDto objects
        }

        // Retrieves a specific TimeLog by its unique ID
        public async Task<TimeLogDto?> GetTimeLog(int timeLogId)
        {
            // Fetches a TimeLog matching the specified ID from the database
            var timeLog = await _context.TimeLogs
                .SingleOrDefaultAsync(t => t.Id == timeLogId); // Returns the first match or null if none found

            if (timeLog == null)
            {
                return null; // Returns null if no matching TimeLog is found
            }

            // Maps the TimeLog to a DTO, including its associated entries
            return new TimeLogDto
            {
                Id = timeLog.Id,
                UserId = timeLog.UserId,
                Title = timeLog.Title,
                CreatedAt = timeLog.CreatedAt,
                TimeLogEntries = await GetTimeLogEntries(timeLog.Id)
            };
        }

        // Retrieves or creates a TimeLog for the current week for a specific user
        public async Task<TimeLogDto> GetOrCreateTimeLogForCurrentWeek(int userId)
        {
            var startOfWeek = GetStartOfWeek(DateTime.Now); // Calculates the start of the current week

            // Looks for an existing TimeLog created on or after the start of the week
            var existingTimeLog = await _context.TimeLogs.Include(t => t.TimeLogEntries)
                .SingleOrDefaultAsync(t => t.UserId == userId && t.CreatedAt >= startOfWeek);

            if (existingTimeLog != null) // If a TimeLog exists, map it to a DTO and return it
            {
                return new TimeLogDto
                {
                    Id = existingTimeLog.Id,
                    UserId = existingTimeLog.UserId,
                    Title = existingTimeLog.Title,
                    CreatedAt = existingTimeLog.CreatedAt,
                    TimeLogEntries = await GetTimeLogEntries(existingTimeLog.Id)
                };
            }

            // If no TimeLog exists, create a new one
            var newTimeLog = new TimeLog
            {
                UserId = userId, // Sets the owner of the TimeLog
                Title = $"Weekly Log for {startOfWeek:MMMM dd, yyyy}", // Sets a descriptive title for the week
                CreatedAt = DateTime.Now // Sets the creation timestamp
            };

            _context.TimeLogs.Add(newTimeLog); // Adds the new TimeLog to the database context
            await _context.SaveChangesAsync(); // Persists changes to the database

            // Maps the newly created TimeLog to a DTO
            return new TimeLogDto
            {
                Id = newTimeLog.Id,
                UserId = newTimeLog.UserId,
                Title = newTimeLog.Title,
                CreatedAt = newTimeLog.CreatedAt
            };
        }

        // Adds a TimeLogEntry to the current week's TimeLog for the user
        public async Task AddTimeLogEntryForCurrentWeek(int userId, int duration, string description)
        {
            // Ensures a TimeLog exists for the current week, creating one if necessary
            var currentWeekLog = await GetOrCreateTimeLogForCurrentWeek(userId);

            // Creates a new TimeLogEntry with the provided data
            var timeLogEntry = new TimeLogEntry
            {
                TimeLogId = currentWeekLog.Id, // Links the entry to the current week's TimeLog
                Duration = duration, // Sets the duration of the entry
                Description = description, // Sets the description of the entry
                CreatedAt = DateTime.Now // Sets the creation timestamp
            };

            _context.TimeLogEntries.Add(timeLogEntry); // Adds the entry to the database context
            await _context.SaveChangesAsync(); // Persists changes to the database
        }

        // Retrieves all TimeLogEntries for a specific TimeLog
        public async Task<List<TimeLogEntryDto>> GetTimeLogEntries(int timeLogId)
        {
            // Fetches all entries for the specified TimeLog ID
            var timeEntries = await _context.TimeLogEntries
                .Where(t => t.TimeLogId == timeLogId) // Filters entries by TimeLogId
                .ToListAsync(); // Executes the query asynchronously

            var timeLogEntryDtos = new List<TimeLogEntryDto>(); // Prepares a list to store DTOs

            // Maps each TimeLogEntry to a DTO
            foreach (var entry in timeEntries)
            {
                var timeLogEntryDto = new TimeLogEntryDto
                {
                    Id = entry.Id,
                    Duration = entry.Duration, // Maps the duration of the entry
                    CreatedAt = entry.CreatedAt, // Maps the creation timestamp
                    Description = entry.Description // Maps the description
                };
                timeLogEntryDtos.Add(timeLogEntryDto); // Adds the mapped DTO to the list
            }

            return timeLogEntryDtos; // Returns the list of DTOs
        }

        // Updates a specific TimeLogEntry by its ID
        public async Task UpdateTimeLogEntry(int timeLogEntryId, int duration, string description)
        {
            // Finds the entry by its ID in the database
            var timeLogEntry = await _context.TimeLogEntries.FindAsync(timeLogEntryId);

            if (timeLogEntry != null) // If the entry exists, update its properties
            {
                timeLogEntry.Duration = duration; // Updates the duration
                timeLogEntry.Description = description; // Updates the description
                await _context.SaveChangesAsync(); // Persists changes to the database
            }
        }

        // Deletes a specific TimeLogEntry by its ID
        public async Task DeleteTimeLogEntry(int timeLogEntryId)
        {
            // Finds the entry by its ID in the database
            var timeLogEntry = await _context.TimeLogEntries.FindAsync(timeLogEntryId);

            if (timeLogEntry != null) // If the entry exists, remove it from the database context
            {
                _context.TimeLogEntries.Remove(timeLogEntry);
                await _context.SaveChangesAsync(); // Persists changes to the database
            }
        }

        // Retrieves a specific TimeLogEntry by its unique ID
        public async Task<TimeLogEntry?> GetTimeLogEntry(int timeLogEntryId)
        {
            // Queries the TimeLogEntries table and includes the related TimeLog
            return await _context.TimeLogEntries.Include(t => t.TimeLog) // Eagerly loads the related TimeLog
                .SingleOrDefaultAsync(t => t.Id == timeLogEntryId); // Returns the entry if found, otherwise null
        }
    }
}