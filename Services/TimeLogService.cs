using Microsoft.EntityFrameworkCore;
using TimeTracker.Data;
using TimeTracker.Models;
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

    // Method to get or create the TimeLog for the current week
    public async Task<TimeLog> GetOrCreateTimeLogForCurrentWeek(int userId)
    {
        var startOfWeek = GetStartOfWeek(DateTime.Now);
        var existingTimeLog = await _context.TimeLogs
            .SingleOrDefaultAsync(t => t.UserId == userId && t.CreatedAt >= startOfWeek);

        if (existingTimeLog != null)
        {
            return existingTimeLog;
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
        return newTimeLog;
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

}