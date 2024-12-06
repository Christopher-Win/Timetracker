// *************************************************
// ***************** Written by: *******************
// ****************** Aayush P. ********************
// *************************************************

using Microsoft.EntityFrameworkCore; // Provides ORM functionality for interacting with the database
using TimeTracker.Data; // References the application's database context
using TimeTracker.Models; // References application models like PeerReviewAnswer and PeerReviewQuestion

// Implementation of the IPeerReviewAnswerService interface
// Handles operations related to PeerReviewAnswers and associated entities
public class PeerReviewAnswerService : IPeerReviewAnswerService
{
    private readonly ApplicationDBContext _context; // Database context for accessing and managing data

    // Constructor to inject the database context
    public PeerReviewAnswerService(ApplicationDBContext context)
    {
        _context = context; // Assigns the injected database context to a private field
    }

    // Creates a new PeerReviewAnswer and saves it to the database
    public async Task CreatePeerReviewAnswerAsync(PeerReviewAnswer answer)
    {
        _context.PeerReviewAnswers.Add(answer); // Adds the answer to the PeerReviewAnswers table
        await _context.SaveChangesAsync(); // Persists changes to the database asynchronously
    }

    // Retrieves all PeerReviewAnswers associated with a specific PeerReview ID
    public async Task<IEnumerable<PeerReviewAnswer>> GetAnswersByPeerReviewIdAsync(int peerReviewId)
    {
        // Fetches answers where the PeerReviewId matches the provided ID
        // Includes PeerReviewQuestion navigation property to retrieve question details if needed
        return await _context.PeerReviewAnswers
            .Where(a => a.PeerReviewId == peerReviewId) // Filters answers by PeerReviewId
            .Include(a => a.PeerReviewQuestion) // Eager loads the associated question for each answer
            .ToListAsync(); // Executes the query and returns the results as a list
    }

    // Retrieves a specific PeerReviewQuestion by its unique ID
    public async Task<PeerReviewQuestion?> GetPeerReviewQuestionByIdAsync(int questionId)
    {
        // Uses FindAsync to locate a PeerReviewQuestion by its primary key (ID)
        return await _context.PeerReviewQuestions.FindAsync(questionId);
    }

    // Retrieves all PeerReviews for a specific reviewer-reviewee pair
    public async Task<IEnumerable<PeerReview>> GetPeerReviewsByReviewerAndRevieweeAsync(string reviewerId, string revieweeId)
    {
        // Fetches PeerReviews where the ReviewerId and RevieweeId match the provided IDs
        // Includes Reviewer and Reviewee navigation properties for detailed user information
        return await _context.PeerReviews
            .Include(pr => pr.Reviewer) // Eager loads the Reviewer entity
            .Include(pr => pr.Reviewee) // Eager loads the Reviewee entity
            .Where(pr => pr.ReviewerId == reviewerId && pr.RevieweeId == revieweeId) // Filters by reviewer and reviewee IDs
            .OrderByDescending(pr => pr.SubmittedAt) // Orders the reviews by the submission date, descending
            .ToListAsync(); // Executes the query and returns the results as a list
    }
}