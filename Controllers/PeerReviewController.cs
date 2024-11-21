using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TimeTracker.Models;
using TimeTracker.Services;
using Microsoft.AspNetCore.Authorization;
using TimeTracker.Models.Dto;
using TimeTracker.Data;
using System.Text.Json;

namespace TimeTracker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PeerReviewController : ControllerBase
    {
        private readonly IPeerReviewService _peerReviewService;
        private readonly IUserService _userService;
        private readonly IPeerReviewAnswerService _peerReviewAnswerService;
        private readonly ApplicationDBContext _context;

        public PeerReviewController(
            IPeerReviewService peerReviewService,
            IUserService userService,
            IPeerReviewAnswerService peerReviewAnswerService,
            ApplicationDBContext context)
        {
            _peerReviewService = peerReviewService;
            _userService = userService;
            _peerReviewAnswerService = peerReviewAnswerService;
            _context = context;
        }

        // GET: api/peerreview/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPeerReview(int id)
        {
            var peerReview = await _peerReviewService.GetPeerReviewByIdAsync(id);

            if (peerReview == null)
            {
                return NotFound("Peer review not found.");
            }

            // Retrieve the associated answers
            var answers = await _peerReviewAnswerService.GetAnswersByPeerReviewIdAsync(id);

            // Prepare the response object
            var response = new
            {
                peerReview.PeerReviewId,
                peerReview.ReviewerId,
                ReviewerName = $"{peerReview.Reviewer.FirstName} {peerReview.Reviewer.LastName}",
                peerReview.RevieweeId,
                RevieweeName = $"{peerReview.Reviewee.FirstName} {peerReview.Reviewee.LastName}",
                peerReview.StartDate,
                peerReview.EndDate,
                peerReview.SubmittedAt,
                Answers = answers.Select(answer => new
                {
                    answer.PeerReviewQuestionId,
                    QuestionText = answer.PeerReviewQuestion.QuestionText,
                    answer.NumericalFeedback,
                    answer.WrittenFeedback
                })
            };

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePeerReview(
            [FromForm] string revieweeId,
            [FromForm] DateTime startDate,
            [FromForm] DateTime endDate,
            [FromForm] string answers)
        {
            try
            {
                // Retrieve the current user's NetID
                var currentNetId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(currentNetId))
                {
                    return Unauthorized("User NetID not found in the token.");
                }

                // Ensure current user exists
                var currentUser = await _userService.GetUserByNetIdAsync(currentNetId);
                if (currentUser == null)
                {
                    return Unauthorized("Current user not found.");
                }

                // Ensure reviewee exists
                var reviewee = await _userService.GetUserByNetIdAsync(revieweeId);
                if (reviewee == null)
                {
                    return BadRequest("The reviewee does not exist.");
                }

                // Validate that the reviewer and reviewee are in the same group
                var areInSameGroup = await _userService.AreUsersInSameGroupAsync(currentUser.NetID, revieweeId);
                if (!areInSameGroup)
                {
                    return BadRequest("You can only review users in the same group.");
                }

                // Create the peer review
                var peerReview = new PeerReview
                {
                    ReviewerId = currentUser.NetID,
                    Reviewer = currentUser,
                    RevieweeId = revieweeId,
                    Reviewee = reviewee,
                    StartDate = startDate,
                    EndDate = endDate,
                    SubmittedAt = DateTime.Now
                };

                await _peerReviewService.CreatePeerReviewAsync(peerReview);

                // Parse answers string into a collection of PeerReviewAnswerDto
                var parsedAnswers = System.Text.Json.JsonSerializer.Deserialize<List<PeerReviewAnswerDto>>(answers);
                if (parsedAnswers == null || !parsedAnswers.Any())
                {
                    return BadRequest("No answers provided.");
                }

                // Save answers
                foreach (var answerDto in parsedAnswers)
                {
                    var peerReviewQuestion = await _peerReviewAnswerService.GetPeerReviewQuestionByIdAsync(answerDto.PeerReviewQuestionId);
                    if (peerReviewQuestion == null)
                    {
                        return BadRequest($"Peer review question with ID {answerDto.PeerReviewQuestionId} not found.");
                    }

                    var answer = new PeerReviewAnswer
                    {
                        PeerReviewId = peerReview.PeerReviewId,
                        PeerReview = peerReview,
                        PeerReviewQuestionId = answerDto.PeerReviewQuestionId,
                        PeerReviewQuestion = peerReviewQuestion,
                        NumericalFeedback = answerDto.NumericalFeedback,
                        WrittenFeedback = answerDto.WrittenFeedback
                    };

                    await _peerReviewAnswerService.CreatePeerReviewAnswerAsync(answer);
                }

                return CreatedAtAction(nameof(GetPeerReview), new { id = peerReview.PeerReviewId }, peerReview);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE: api/peerreview/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePeerReview(int id)
        {
            try
            {
                // Retrieve the current user's NetID from the claims (authenticated user)
                var currentNetId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(currentNetId))
                {
                    return Unauthorized("User NetID not found in the token.");
                }

                // Retrieve the peer review by ID
                var peerReview = await _peerReviewService.GetPeerReviewByIdAsync(id);

                if (peerReview == null)
                {
                    return NotFound("Peer review not found.");
                }

                // Check if the current user is the reviewer (or any other logic to authorize deletion)
                if (peerReview.ReviewerId != currentNetId)
                {
                    return Forbid("You are not authorized to delete this peer review.");
                }

                // Delete the peer review
                await _peerReviewService.DeletePeerReviewAsync(id);

                return Ok($"Peer review {id} has been deleted"); // after successful deletion
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}