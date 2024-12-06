// Written by: Aayush P.
using Microsoft.AspNetCore.Mvc; // Provides functionality for building RESTful APIs
using TimeTracker.Models; // References application models like PeerReviewQuestion
using TimeTracker.Services; // Includes service interfaces and implementations for PeerReviewQuestion
using Microsoft.AspNetCore.Authorization; // Enables role-based or policy-based access control

namespace TimeTracker.Controllers
{
    // Base URL: api/PeerReviewQuestion
    [Route("api/[controller]")]
    [ApiController] // Indicates this is a RESTful API controller
    public class PeerReviewQuestionController : ControllerBase
    {
        private readonly IPeerReviewQuestionService _peerReviewQuestionService; // Handles PeerReviewQuestion operations

        // Constructor for dependency injection
        // The service for managing PeerReviewQuestion is injected into the controller
        public PeerReviewQuestionController(IPeerReviewQuestionService peerReviewQuestionService)
        {
            _peerReviewQuestionService = peerReviewQuestionService; // Assigns the injected service to the private field
        }

        // GET: api/peerreviewquestion
        // Retrieves all peer review questions from the database
        [HttpGet]
        public async Task<IActionResult> GetAllPeerReviewQuestions()
        {
            // Fetch all peer review questions asynchronously from the service
            var questions = await _peerReviewQuestionService.GetAllPeerReviewQuestionsAsync();
            return Ok(questions); // Return HTTP 200 OK with the list of questions
        }

        // GET: api/peerreviewquestion/{id}
        // Retrieves a specific peer review question by its unique ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPeerReviewQuestionById(int id)
        {
            // Fetch the peer review question using the provided ID
            var question = await _peerReviewQuestionService.GetPeerReviewQuestionByIdAsync(id);
            if (question == null) // Check if the question exists
            {
                return NotFound("Peer review question not found."); // HTTP 404 Not Found if the question doesn't exist
            }
            return Ok(question); // Return HTTP 200 OK with the question details
        }

        // POST: api/peerreviewquestion
        // Creates a new peer review question
        [HttpPost]
        //[Authorize(Roles = "Admin")] // Uncomment to restrict this action to Admin users
        public async Task<IActionResult> CreatePeerReviewQuestion([FromForm] PeerReviewQuestion peerReviewQuestion)
        {
            // Validate that the question object is not null and contains valid data
            if (peerReviewQuestion == null || string.IsNullOrEmpty(peerReviewQuestion.QuestionText))
            {
                return BadRequest("Question text is required."); // HTTP 400 Bad Request if validation fails
            }

            try
            {
                // Save the new peer review question using the service
                await _peerReviewQuestionService.CreatePeerReviewQuestionAsync(peerReviewQuestion);

                // Return HTTP 201 Created with the details of the newly created question
                return CreatedAtAction(nameof(GetPeerReviewQuestionById), new { id = peerReviewQuestion.PeerReviewQuestionId }, peerReviewQuestion);
            }
            catch (Exception ex) // Handle unexpected exceptions
            {
                return StatusCode(500, $"Internal server error: {ex.Message}"); // HTTP 500 Internal Server Error with the exception message
            }
        }

        // DELETE: api/peerreviewquestion/{id}
        // Deletes a specific peer review question by its unique ID
        [HttpDelete("{id}")]
        //[Authorize(Roles = "Admin")] // Uncomment to restrict this action to Admin users
        public async Task<IActionResult> DeletePeerReviewQuestion(int id)
        {
            // Fetch the peer review question to verify it exists
            var question = await _peerReviewQuestionService.GetPeerReviewQuestionByIdAsync(id);
            if (question == null) // Check if the question exists
            {
                return NotFound("Peer review question not found."); // HTTP 404 Not Found if the question doesn't exist
            }
            try
            {
                // Delete the peer review question using the service
                await _peerReviewQuestionService.DeletePeerReviewQuestionAsync(id);

                // Return HTTP 200 OK with a success message
                return Ok(new { message = $"Question {id} deleted" });
            }
            catch (Exception ex) // Handle unexpected exceptions
            {
                return StatusCode(500, $"Internal server error: {ex.Message}"); // HTTP 500 Internal Server Error with the exception message
            }
        }
    }
}