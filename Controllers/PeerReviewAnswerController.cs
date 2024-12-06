// *************************************************
// ***************** Written by: *******************
// ****************** Aayush P. ********************
// *************************************************

using Microsoft.AspNetCore.Mvc;
using TimeTracker.Services;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Data;

[Route("api/[controller]")]
[ApiController]
public class PeerReviewAnswerController : ControllerBase
{
    private readonly IPeerReviewAnswerService _peerReviewAnswerService;
    private readonly IPeerReviewService _peerReviewService;
    private readonly IUserService _userService;
    private readonly ApplicationDBContext _context;

    public PeerReviewAnswerController(
        IPeerReviewAnswerService peerReviewAnswerService, 
        IPeerReviewService peerReviewService, 
        IUserService userService,
        ApplicationDBContext context)
    {
        _peerReviewAnswerService = peerReviewAnswerService;
        _peerReviewService = peerReviewService;
        _userService = userService;
        _context = context;
    }

    // GET: api/PeerReviewAnswer/reviewer/{reviewerId}/reviewee/{revieweeId}
    [HttpGet("reviewer/{reviewerId}/reviewee/{revieweeId}")]
    public async Task<IActionResult> GetAnswersForReviewerAndReviewee(string reviewerId, string revieweeId)
    {
        // Step 1: Fetch the relevant peer reviews
        var peerReviews = await _context.PeerReviews
            .AsNoTracking()
            .Where(pr => pr.ReviewerId == reviewerId && pr.RevieweeId == revieweeId)
            .OrderByDescending(pr => pr.SubmittedAt)
            .Select(pr => new
            {
                PeerReviewId = pr.PeerReviewId,
                ReviewerName = $"{pr.Reviewer.FirstName} {pr.Reviewer.LastName}",
                RevieweeName = $"{pr.Reviewee.FirstName} {pr.Reviewee.LastName}",
                pr.SubmittedAt
            })
            .ToListAsync();

        if (!peerReviews.Any())
        {
            return NotFound("No peer reviews found between the specified reviewer and reviewee.");
        }

        // Step 2: Fetch all answers for the peer reviews
        var peerReviewIds = peerReviews.Select(pr => pr.PeerReviewId).ToList();
        var peerReviewAnswers = await _context.PeerReviewAnswers
            .AsNoTracking()
            .Where(answer => peerReviewIds.Contains(answer.PeerReviewId))
            .Include(answer => answer.PeerReviewQuestion) // Include the question details
            .ToListAsync();

        // Step 3: Construct the response
        var response = peerReviews.Select(pr => new
        {
            pr.PeerReviewId,
            pr.ReviewerName,
            pr.RevieweeName,
            pr.SubmittedAt,
            Answers = peerReviewAnswers
                .Where(answer => answer.PeerReviewId == pr.PeerReviewId)
                .Select(answer => new
                {
                    Question = answer.PeerReviewQuestion.QuestionText,
                    answer.NumericalFeedback,
                    answer.WrittenFeedback
                })
        });

        return Ok(response);
    }
}