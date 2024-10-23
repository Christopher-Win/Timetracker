using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TimeTracker.Models;
using TimeTracker.Services;
using Microsoft.AspNetCore.Authorization;

namespace TimeTracker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PeerReviewController : ControllerBase
    {
        private readonly IPeerReviewService _peerReviewService;
        private readonly IUserService _userService;

        
        public PeerReviewController (IPeerReviewService peerReviewService, IUserService userService)
        {
            _peerReviewService = peerReviewService;
            _userService = userService;
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

            return Ok(peerReview);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePeerReview([FromBody] PeerReview peerReview)
        {
            try
            {
                // Retrieve the current user's NetID from the claims (authenticated user)
                var currentNetId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Ensure current user exists
                var currentUser = await _userService.GetUserByNetIdAsync(currentNetId);
                if (currentUser == null)
                {
                    return Unauthorized("Current user not found.");
                }

                // Automatically assign the reviewerId as the current authenticated user's ID
                peerReview.ReviewerId = currentUser.Id;

                // Validate that the reviewer and reviewee are in the same group
                var areInSameGroup = await _userService.AreUsersInSameGroupAsync(currentUser.Id, peerReview.RevieweeId);
                if (!areInSameGroup)
                {
                    return BadRequest("You can only review users in the same group.");
                }

                // Create the peer review if validation passes
                await _peerReviewService.CreatePeerReviewAsync(peerReview);
                return CreatedAtAction(nameof(GetPeerReview), new { id = peerReview.PeerReviewId }, peerReview);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}