using Microsoft.EntityFrameworkCore; // Provides ORM functionality for querying and interacting with the database
using TimeTracker.Data; // References the application's database context
using TimeTracker.Models; // References application models like PeerReview, Reviewer, and Reviewee

namespace TimeTracker.Services
{
    // Implementation of the IPeerReviewService interface
    // Handles operations related to managing PeerReviews
    public class PeerReviewService : IPeerReviewService
    {
        private readonly ApplicationDBContext _context; // Database context for accessing and managing data

        // Constructor to inject the database context
        public PeerReviewService(ApplicationDBContext context)
        {
            _context = context; // Assigns the injected database context to a private field
        }

        // Retrieves a specific PeerReview by its unique ID
        public async Task<PeerReview?> GetPeerReviewByIdAsync(int id)
        {
            // Queries the PeerReviews table and includes Reviewer and Reviewee navigation properties
            return await _context.PeerReviews
                .Include(pr => pr.Reviewer) // Eagerly loads the Reviewer entity
                .Include(pr => pr.Reviewee) // Eagerly loads the Reviewee entity
                .FirstOrDefaultAsync(pr => pr.PeerReviewId == id); // Returns the first match or null if none found
        }

        // Retrieves a PeerReview for a specific reviewer-reviewee pair
        public async Task<PeerReview?> GetPeerReviewByReviewerAndRevieweeAsync(string reviewerId, string revieweeId)
        {
            // Queries the PeerReviews table for a specific reviewer-reviewee pair
            return await _context.PeerReviews
                .Include(pr => pr.Reviewer) // Eagerly loads the Reviewer entity
                .Include(pr => pr.Reviewee) // Eagerly loads the Reviewee entity
                .FirstOrDefaultAsync(pr => pr.ReviewerId == reviewerId && pr.RevieweeId == revieweeId); // Filters by ReviewerId and RevieweeId
        }

        // Creates a new PeerReview and saves it to the database
        public async Task CreatePeerReviewAsync(PeerReview peerReview)
        {
            _context.PeerReviews.Add(peerReview); // Adds the new PeerReview to the context
            await _context.SaveChangesAsync(); // Persists changes to the database asynchronously
        }

        // Deletes a specific PeerReview by its unique ID
        public async Task DeletePeerReviewAsync(int id)
        {
            // Finds the PeerReview by its ID
            var peerReview = await _context.PeerReviews.FindAsync(id);
            if (peerReview != null) // Checks if the PeerReview exists
            {
                _context.PeerReviews.Remove(peerReview); // Removes the PeerReview from the context
                await _context.SaveChangesAsync(); // Persists the deletion to the database asynchronously
            }
        }
    }
}