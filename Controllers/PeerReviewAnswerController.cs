// Written by: Aayush P.
using Microsoft.AspNetCore.Mvc; // Provides functionality for building RESTful APIs
using TimeTracker.Services; // Includes service classes for PeerReview and related operations
using Microsoft.AspNetCore.Authorization; // Enables role-based or policy-based access control

// Route configuration for the API controller
// Base URL for this controller: api/PeerReviewAnswer
[Route("api/[controller]")]
[ApiController] // Indicates this is a RESTful API controller
[Authorize] // Ensures that only authenticated users can access the endpoints
public class PeerReviewAnswerController : ControllerBase
{
    private readonly IPeerReviewAnswerService _peerReviewAnswerService; // Handles Peer Review Answer operations
    private readonly IPeerReviewService _peerReviewService; // Handles Peer Review operations
    private readonly IUserService _userService; // Handles user-related operations

    // Constructor for injecting the required services into the controller
    // Dependency Injection ensures that these services are provided at runtime
    public PeerReviewAnswerController(
        IPeerReviewAnswerService peerReviewAnswerService, 
        IPeerReviewService peerReviewService, 
        IUserService userService)
    {
        _peerReviewAnswerService = peerReviewAnswerService; // Assigns the injected PeerReviewAnswerService
        _peerReviewService = peerReviewService; // Assigns the injected PeerReviewService
        _userService = userService; // Assigns the injected UserService
    }

    // GET: api/PeerReviewAnswer/reviewer/{reviewerId}/reviewee/{revieweeId}
    // Retrieves all peer review answers for a specific reviewer and reviewee
    [HttpGet("reviewer/{reviewerId}/reviewee/{revieweeId}")]
    public async Task<IActionResult> GetAnswersForReviewerAndReviewee(string reviewerId, string revieweeId)
    {
        // Step 1: Validate the existence of the reviewer
        var reviewer = await _userService.GetUserByNetIdAsync(reviewerId); // Fetch the reviewer using their NetID
        if (reviewer == null) // If reviewer doesn't exist, return 404
        {
            return NotFound($"Reviewer with ID {reviewerId} not found.");
        }

        // Step 2: Validate the existence of the reviewee
        var reviewee = await _userService.GetUserByNetIdAsync(revieweeId); // Fetch the reviewee using their NetID
        if (reviewee == null) // If reviewee doesn't exist, return 404
        {
            return NotFound($"Reviewee with ID {revieweeId} not found.");
        }

        // Step 3: Retrieve all PeerReview records for the reviewer-reviewee pair
        // Peer reviews are ordered by their submission date in descending order
        var peerReviews = await _peerReviewAnswerService.GetPeerReviewsByReviewerAndRevieweeAsync(reviewerId, revieweeId);
        if (!peerReviews.Any()) // If no peer reviews exist, return 404
        {
            return NotFound("No peer reviews found between the specified reviewer and reviewee.");
        }

        // Step 4: Retrieve all answers for each PeerReview
        // Create a response list to hold details for each PeerReview and its answers
        var response = new List<object>();

        foreach (var peerReview in peerReviews)
        {
            // Fetch all answers associated with the current PeerReview ID
            var answers = await _peerReviewAnswerService.GetAnswersByPeerReviewIdAsync(peerReview.PeerReviewId);

            // Build a response object for the current PeerReview
            response.Add(new
            {
                PeerReviewId = peerReview.PeerReviewId, // Unique ID of the PeerReview
                ReviewerName = $"{peerReview.Reviewer.FirstName} {peerReview.Reviewer.LastName}", // Full name of the reviewer
                RevieweeName = $"{peerReview.Reviewee.FirstName} {peerReview.Reviewee.LastName}", // Full name of the reviewee
                peerReview.SubmittedAt, // Date and time when the review was submitted
                Answers = answers.Select(answer => new // For each answer, include the question and feedback
                {
                    Question = answer.PeerReviewQuestion.QuestionText, // The question text
                    answer.NumericalFeedback, // Numerical feedback provided
                    answer.WrittenFeedback // Written feedback provided
                })
            });
        }

        // Step 5: Return the response containing PeerReview details and associated answers
        return Ok(response); // HTTP 200 OK with the constructed response object
    }
}