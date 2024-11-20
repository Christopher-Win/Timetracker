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

    // POST: api/peerreviewanswer
    [HttpPost]
    public async Task<IActionResult> CreatePeerReviewAnswer([FromForm] PeerReviewAnswerDto answerDto)
    {
        // Check if the related peer review exists
        var peerReview = await _peerReviewService.GetPeerReviewByIdAsync(answerDto.PeerReviewId);
        if (peerReview == null)
        {
            return NotFound("Peer review not found.");
        }

        // Check if the related question exists
        var peerReviewQuestion = await _peerReviewAnswerService.GetPeerReviewQuestionByIdAsync(answerDto.PeerReviewQuestionId);
        if (peerReviewQuestion == null)
        {
            return NotFound("Peer review question not found.");
        }

        // Create the PeerReviewAnswer instance
        var answer = new PeerReviewAnswer
        {
            PeerReviewId = answerDto.PeerReviewId,
            PeerReview = peerReview,  // Set the PeerReview entity
            PeerReviewQuestionId = answerDto.PeerReviewQuestionId,
            PeerReviewQuestion = peerReviewQuestion,  // Set the PeerReviewQuestion entity
            NumericalFeedback = answerDto.NumericalFeedback,
            WrittenFeedback = answerDto.WrittenFeedback
        };

        // Save the answer
        await _peerReviewAnswerService.CreatePeerReviewAnswerAsync(answer);
        return Ok(answer);
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

        // Retrieve the most recent PeerReview
        var peerReview = await _peerReviewAnswerService.GetPeerReviewByReviewerAndRevieweeAsync(reviewerId, revieweeId);
        if (peerReview == null)
        {
            return NotFound("No peer review found between the specified reviewer and reviewee.");
        }

        // Retrieve answers for the peer review
        var answers = await _peerReviewAnswerService.GetAnswersByPeerReviewIdAsync(peerReview.PeerReviewId);

        if (!answers.Any())
        {
            return Ok("No answers found for this peer review.");
        }

        // Simplify the response
        var response = answers.Select(answer => new
        {
            Question = answer.PeerReviewQuestion.QuestionText,
            answer.NumericalFeedback,
            answer.WrittenFeedback
        });

        return Ok(response);
    }
}