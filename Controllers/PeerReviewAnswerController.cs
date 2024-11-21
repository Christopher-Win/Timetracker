using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TimeTracker.Models;
using TimeTracker.Services;
using Microsoft.AspNetCore.Authorization;
using TimeTracker.Models.Dto;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PeerReviewAnswerController : ControllerBase
{
    private readonly IPeerReviewAnswerService _peerReviewAnswerService;
    private readonly IPeerReviewService _peerReviewService;
    
    private readonly IUserService _userService;

    public PeerReviewAnswerController(IPeerReviewAnswerService peerReviewAnswerService, IPeerReviewService peerReviewService, IUserService userService)
    {
        _peerReviewAnswerService = peerReviewAnswerService;
        _peerReviewService = peerReviewService;
        _userService = userService;
    }

    [HttpGet("reviewer/{reviewerId}/reviewee/{revieweeId}")]
    public async Task<IActionResult> GetAnswersForReviewerAndReviewee(string reviewerId, string revieweeId)
    {
        // Validate reviewer and reviewee existence
        var reviewer = await _userService.GetUserByNetIdAsync(reviewerId);
        if (reviewer == null)
        {
            return NotFound($"Reviewer with ID {reviewerId} not found.");
        }

        var reviewee = await _userService.GetUserByNetIdAsync(revieweeId);
        if (reviewee == null)
        {
            return NotFound($"Reviewee with ID {revieweeId} not found.");
        }

        // Retrieve all PeerReview records between the reviewer and reviewee, ordered by SubmittedAt (descending)
        var peerReviews = await _peerReviewAnswerService.GetPeerReviewsByReviewerAndRevieweeAsync(reviewerId, revieweeId);
        if (!peerReviews.Any())
        {
            return NotFound("No peer reviews found between the specified reviewer and reviewee.");
        }

        // Retrieve all answers for each PeerReview
        var response = new List<object>();

        foreach (var peerReview in peerReviews)
        {
            var answers = await _peerReviewAnswerService.GetAnswersByPeerReviewIdAsync(peerReview.PeerReviewId);

            response.Add(new
            {
                PeerReviewId = peerReview.PeerReviewId,
                ReviewerName = $"{peerReview.Reviewer.FirstName} {peerReview.Reviewer.LastName}",
                RevieweeName = $"{peerReview.Reviewee.FirstName} {peerReview.Reviewee.LastName}",
                peerReview.SubmittedAt,
                Answers = answers.Select(answer => new
                {
                    Question = answer.PeerReviewQuestion.QuestionText,
                    answer.NumericalFeedback,
                    answer.WrittenFeedback
                })
            });
        }

        return Ok(response);
    }
}