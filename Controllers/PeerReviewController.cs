// Written by: Aayush P.
using Microsoft.AspNetCore.Mvc; // Provides functionality for building RESTful APIs
using System.Security.Claims; // Enables extraction of claims from the authenticated user's JWT
using TimeTracker.Models; // References application models like PeerReview
using TimeTracker.Services; // Includes service interfaces and implementations
using Microsoft.AspNetCore.Authorization; // Provides attributes for role-based or policy-based access control
using TimeTracker.Models.Dto; // References Data Transfer Objects for PeerReviewAnswer
using TimeTracker.Data; // Includes application data context for database access

namespace TimeTracker.Controllers
{
    // Base URL: api/PeerReview
    [Route("api/[controller]")]
    [ApiController] // Indicates this is a RESTful API controller
    [Authorize] // Ensures only authenticated users can access endpoints
    public class PeerReviewController : ControllerBase
    {
        private readonly IPeerReviewService _peerReviewService; // Handles peer review operations
        private readonly IUserService _userService; // Handles user-related operations
        private readonly IPeerReviewAnswerService _peerReviewAnswerService; // Handles peer review answers
        private readonly ApplicationDBContext _context; // Provides direct access to the database

        // Constructor for injecting dependencies
        // Dependency injection ensures services are initialized at runtime
        public PeerReviewController(
            IPeerReviewService peerReviewService,
            IUserService userService,
            IPeerReviewAnswerService peerReviewAnswerService,
            ApplicationDBContext context)
        {
            _peerReviewService = peerReviewService; // Assigns the peer review service
            _userService = userService; // Assigns the user service
            _peerReviewAnswerService = peerReviewAnswerService; // Assigns the peer review answer service
            _context = context; // Assigns the database context
        }

        // GET: api/peerreview/{id}
        // Retrieves a peer review by its unique ID, including associated answers
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPeerReview(int id)
        {
            // Fetch the peer review using the provided ID
            var peerReview = await _peerReviewService.GetPeerReviewByIdAsync(id);

            if (peerReview == null) // Check if the peer review exists
            {
                return NotFound("Peer review not found."); // Return HTTP 404 if not found
            }

            // Retrieve all answers associated with the peer review
            var answers = await _peerReviewAnswerService.GetAnswersByPeerReviewIdAsync(id);

            // Prepare the response object containing peer review and answer details
            var response = new
            {
                peerReview.PeerReviewId,
                peerReview.ReviewerId,
                ReviewerName = $"{peerReview.Reviewer.FirstName} {peerReview.Reviewer.LastName}", // Full reviewer name
                peerReview.RevieweeId,
                RevieweeName = $"{peerReview.Reviewee.FirstName} {peerReview.Reviewee.LastName}", // Full reviewee name
                peerReview.StartDate,
                peerReview.EndDate,
                peerReview.SubmittedAt,
                Answers = answers.Select(answer => new
                {
                    answer.PeerReviewQuestionId, // ID of the associated question
                    QuestionText = answer.PeerReviewQuestion.QuestionText, // Text of the question
                    answer.NumericalFeedback, // Numerical rating provided
                    answer.WrittenFeedback // Written feedback provided
                })
            };

            return Ok(response); // Return HTTP 200 with the response object
        }

        // POST: api/peerreview
        // Creates a new peer review and saves associated answers
        [HttpPost]
        public async Task<IActionResult> CreatePeerReview(
            [FromForm] string revieweeId, // ID of the user being reviewed
            [FromForm] DateTime startDate, // Start date of the review
            [FromForm] DateTime endDate, // End date of the review
            [FromForm] string answers) // JSON string of answers
        {
            try
            {
                // Retrieve the current user's NetID from the authenticated JWT
                var currentNetId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(currentNetId)) // Validate the NetID
                {
                    return Unauthorized("User NetID not found in the token."); // HTTP 401 Unauthorized
                }

                // Ensure the current user exists in the system
                var currentUser = await _userService.GetUserByNetIdAsync(currentNetId);
                if (currentUser == null)
                {
                    return Unauthorized("Current user not found."); // HTTP 401 Unauthorized
                }

                // Ensure the reviewee exists in the system
                var reviewee = await _userService.GetUserByNetIdAsync(revieweeId);
                if (reviewee == null)
                {
                    return BadRequest("The reviewee does not exist."); // HTTP 400 Bad Request
                }

                // Validate that the reviewer and reviewee belong to the same group
                var areInSameGroup = await _userService.AreUsersInSameGroupAsync(currentUser.NetID, revieweeId);
                if (!areInSameGroup)
                {
                    return BadRequest("You can only review users in the same group."); // HTTP 400 Bad Request
                }

                // Create a new PeerReview object
                var peerReview = new PeerReview
                {
                    ReviewerId = currentUser.NetID,
                    Reviewer = currentUser,
                    RevieweeId = revieweeId,
                    Reviewee = reviewee,
                    StartDate = startDate,
                    EndDate = endDate,
                    SubmittedAt = DateTime.Now // Record the submission time
                };

                // Save the peer review to the database
                await _peerReviewService.CreatePeerReviewAsync(peerReview);

                // Parse the answers JSON string into a list of PeerReviewAnswerDto
                var parsedAnswers = System.Text.Json.JsonSerializer.Deserialize<List<PeerReviewAnswerDto>>(answers);
                if (parsedAnswers == null || !parsedAnswers.Any()) // Validate the parsed answers
                {
                    return BadRequest("No answers provided."); // HTTP 400 Bad Request
                }

                // Iterate through the parsed answers and save them
                foreach (var answerDto in parsedAnswers)
                {
                    // Fetch the peer review question by its ID
                    var peerReviewQuestion = await _peerReviewAnswerService.GetPeerReviewQuestionByIdAsync(answerDto.PeerReviewQuestionId);
                    if (peerReviewQuestion == null) // Validate the question
                    {
                        return BadRequest($"Peer review question with ID {answerDto.PeerReviewQuestionId} not found.");
                    }

                    // Create a PeerReviewAnswer object and save it
                    var answer = new PeerReviewAnswer
                    {
                        PeerReviewId = peerReview.PeerReviewId,
                        PeerReview = peerReview,
                        PeerReviewQuestionId = answerDto.PeerReviewQuestionId,
                        PeerReviewQuestion = peerReviewQuestion,
                        NumericalFeedback = answerDto.NumericalFeedback,
                        WrittenFeedback = answerDto.WrittenFeedback
                    };

                    await _peerReviewAnswerService.CreatePeerReviewAnswerAsync(answer); // Save the answer
                }

                // Return HTTP 201 Created with the newly created PeerReview details
                return CreatedAtAction(nameof(GetPeerReview), new { id = peerReview.PeerReviewId }, peerReview);
            }
            catch (Exception ex) // Catch unexpected errors
            {
                return StatusCode(500, $"Internal server error: {ex.Message}"); // HTTP 500 Internal Server Error
            }
        }

        // DELETE: api/peerreview/{id}
        // Deletes a peer review by its unique ID
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePeerReview(int id)
        {
            try
            {
                // Retrieve the current user's NetID from the authenticated JWT
                var currentNetId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(currentNetId)) // Validate the NetID
                {
                    return Unauthorized("User NetID not found in the token."); // HTTP 401 Unauthorized
                }

                // Fetch the peer review using the provided ID
                var peerReview = await _peerReviewService.GetPeerReviewByIdAsync(id);

                if (peerReview == null) // Check if the peer review exists
                {
                    return NotFound("Peer review not found."); // HTTP 404 Not Found
                }

                // Validate that the current user is the reviewer
                if (peerReview.ReviewerId != currentNetId)
                {
                    return Forbid("You are not authorized to delete this peer review."); // HTTP 403 Forbidden
                }

                // Delete the peer review
                await _peerReviewService.DeletePeerReviewAsync(id);

                return Ok($"Peer review {id} has been deleted"); // HTTP 200 OK with success message
            }
            catch (Exception ex) // Catch unexpected errors
            {
                return StatusCode(500, $"Internal server error: {ex.Message}"); // HTTP 500 Internal Server Error
            }
        }
    }
}