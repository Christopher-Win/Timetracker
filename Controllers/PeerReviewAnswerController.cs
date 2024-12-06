// *************************************************
// ***************** Written by: *******************
// ****************** Aayush P. ********************
// *************************************************

using Microsoft.AspNetCore.Mvc; // Provides functionality for building RESTful APIs
using TimeTracker.Services; // Includes service classes for PeerReview and related operations
using Microsoft.EntityFrameworkCore; // Provides ORM functionality for querying the database
using TimeTracker.Data; // Includes the application's database context

[Route("api/[controller]")]
[ApiController]
public class PeerReviewAnswerController : ControllerBase
{
    private readonly IPeerReviewAnswerService _peerReviewAnswerService; // Service for managing PeerReviewAnswer entities
    private readonly IPeerReviewService _peerReviewService; // Service for managing PeerReview entities
    private readonly IUserService _userService; // Service for managing User entities
    private readonly ApplicationDBContext _context; // Database context for querying and saving data

    public PeerReviewAnswerController(
        IPeerReviewAnswerService peerReviewAnswerService, 
        IPeerReviewService peerReviewService, 
        IUserService userService,
        ApplicationDBContext context)
    {
        _peerReviewAnswerService = peerReviewAnswerService; // Inject PeerReviewAnswerService
        _peerReviewService = peerReviewService; // Inject PeerReviewService
        _userService = userService; // Inject UserService
        _context = context; // Inject the database context
    }

    // GET: api/PeerReviewAnswer/reviewer/{reviewerId}/reviewee/{revieweeId}
    // Retrieves all peer review answers for a specific reviewer and reviewee
    [HttpGet("reviewer/{reviewerId}/reviewee/{revieweeId}")]
    public async Task<IActionResult> GetAnswersForReviewerAndReviewee(string reviewerId, string revieweeId)
    {
        // Step 1: Fetch the relevant peer reviews
        // Query the PeerReviews table for all reviews where the reviewer and reviewee IDs match
        var peerReviews = await _context.PeerReviews
            .AsNoTracking() // Avoid tracking changes since this is a read-only operation
            .Where(pr => pr.ReviewerId == reviewerId && pr.RevieweeId == revieweeId) // Filter by reviewer and reviewee IDs
            .OrderByDescending(pr => pr.SubmittedAt) // Sort by submission date in descending order (most recent first)
            .Select(pr => new // Project only the required fields into an anonymous object
            {
                PeerReviewId = pr.PeerReviewId, // Unique ID of the PeerReview
                ReviewerName = $"{pr.Reviewer.FirstName} {pr.Reviewer.LastName}", // Full name of the reviewer
                RevieweeName = $"{pr.Reviewee.FirstName} {pr.Reviewee.LastName}", // Full name of the reviewee
                pr.SubmittedAt // Submission date of the PeerReview
            })
            .ToListAsync(); // Execute the query asynchronously and fetch the results

        // If no peer reviews exist for the given reviewer-reviewee pair, return a 404 Not Found response
        if (!peerReviews.Any())
        {
            return NotFound("No peer reviews found between the specified reviewer and reviewee.");
        }

        // Step 2: Fetch all answers for the peer reviews
        // Extract the PeerReview IDs from the fetched reviews
        var peerReviewIds = peerReviews.Select(pr => pr.PeerReviewId).ToList();

        // Query the PeerReviewAnswers table for answers belonging to the fetched PeerReview IDs
        var peerReviewAnswers = await _context.PeerReviewAnswers
            .AsNoTracking() // Avoid tracking changes since this is a read-only operation
            .Where(answer => peerReviewIds.Contains(answer.PeerReviewId)) // Filter by PeerReview IDs
            .Include(answer => answer.PeerReviewQuestion) // Include the PeerReviewQuestion entity for each answer
            .ToListAsync(); // Execute the query asynchronously and fetch the results

        // Step 3: Construct the response
        // Combine the fetched peer reviews and their associated answers into a structured response
        var response = peerReviews.Select(pr => new
        {
            pr.PeerReviewId, // PeerReview ID
            pr.ReviewerName, // Full name of the reviewer
            pr.RevieweeName, // Full name of the reviewee
            pr.SubmittedAt, // Submission date of the PeerReview
            Answers = peerReviewAnswers // Filter the answers that belong to the current PeerReview
                .Where(answer => answer.PeerReviewId == pr.PeerReviewId)
                .Select(answer => new // Project only the required fields for each answer into an anonymous object
                {
                    Question = answer.PeerReviewQuestion.QuestionText, // The question text
                    answer.NumericalFeedback, // Numerical feedback provided
                    answer.WrittenFeedback // Written feedback provided
                })
        });

        // Return the constructed response as HTTP 200 OK
        return Ok(response);
    }
}