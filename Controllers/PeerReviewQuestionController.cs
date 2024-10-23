using Microsoft.AspNetCore.Mvc;
using TimeTracker.Models;
using TimeTracker.Services;
using Microsoft.AspNetCore.Authorization;

namespace TimeTracker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PeerReviewQuestionController : ControllerBase
    {
        private readonly IPeerReviewQuestionService _peerReviewQuestionService;

        public PeerReviewQuestionController(IPeerReviewQuestionService peerReviewQuestionService)
        {
            _peerReviewQuestionService = peerReviewQuestionService;
        }

        // GET: api/peerreviewquestion
        [HttpGet]
        public async Task<IActionResult> GetAllPeerReviewQuestions()
        {
            var questions = await _peerReviewQuestionService.GetAllPeerReviewQuestionsAsync();
            return Ok(questions);
        }

        // GET: api/peerreviewquestion/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPeerReviewQuestionById(int id)
        {
            var question = await _peerReviewQuestionService.GetPeerReviewQuestionByIdAsync(id);
            if (question == null)
            {
                return NotFound("Peer review question not found.");
            }
            return Ok(question);
        }

        // POST: api/peerreviewquestion
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreatePeerReviewQuestion([FromForm] PeerReviewQuestion peerReviewQuestion)
        {
            if (peerReviewQuestion == null || string.IsNullOrEmpty(peerReviewQuestion.QuestionText))
            {
                return BadRequest("Question text is required.");
            }

            try
            {
                await _peerReviewQuestionService.CreatePeerReviewQuestionAsync(peerReviewQuestion);
                return CreatedAtAction(nameof(GetPeerReviewQuestionById), new { id = peerReviewQuestion.PeerReviewQuestionId }, peerReviewQuestion);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}