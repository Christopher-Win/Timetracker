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

    public PeerReviewAnswerController(IPeerReviewAnswerService peerReviewAnswerService, IPeerReviewService peerReviewService)
    {
        _peerReviewAnswerService = peerReviewAnswerService;
        _peerReviewService = peerReviewService;
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
        return CreatedAtAction(nameof(GetAnswersForPeerReview), new { peerReviewId = answer.PeerReviewId }, answer);
    }

    // GET: api/peerreviewanswer/peerreview/{peerReviewId}
    [HttpGet("peerreview/{peerReviewId}")]
    public async Task<IActionResult> GetAnswersForPeerReview(int peerReviewId)
    {
        // Check if the related peer review exists
        var peerReview = await _peerReviewService.GetPeerReviewByIdAsync(peerReviewId);
        if (peerReview == null)
        {
            return NotFound("Peer review not found.");
        }

        // Retrieve all answers associated with this peer review
        var answers = await _peerReviewAnswerService.GetAnswersByPeerReviewIdAsync(peerReviewId);

        return Ok(answers);
    }
}