using Microsoft.EntityFrameworkCore;
using TimeTracker.Data;
using TimeTracker.Models;

namespace TimeTracker.Services
{
    public class PeerReviewService : IPeerReviewService
    {
        private readonly ApplicationDBContext _context;

        public PeerReviewService(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<PeerReview?> GetPeerReviewByIdAsync(int id)
        {
            return await _context.PeerReviews
                .Include(pr => pr.Reviewer) // Include the Reviewer entity
                .Include(pr => pr.Reviewee) // Include the Reviewee entity
                .FirstOrDefaultAsync(pr => pr.PeerReviewId == id);
        }

        public async Task<PeerReview?> GetPeerReviewByReviewerAndRevieweeAsync(string reviewerId, string revieweeId)
        {
            return await _context.PeerReviews
                .Include(pr => pr.Reviewer) // Include Reviewer navigation property
                .Include(pr => pr.Reviewee) // Include Reviewee navigation property
                .FirstOrDefaultAsync(pr => pr.ReviewerId == reviewerId && pr.RevieweeId == revieweeId);
        }

        public async Task CreatePeerReviewAsync(PeerReview peerReview)
        {
            _context.PeerReviews.Add(peerReview);
            await _context.SaveChangesAsync();
        }

        public async Task DeletePeerReviewAsync(int id)
        {
            var peerReview = await _context.PeerReviews.FindAsync(id);
            if (peerReview != null)
            {
                _context.PeerReviews.Remove(peerReview);
                await _context.SaveChangesAsync();
            }
        }
    }
}