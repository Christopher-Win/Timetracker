using Microsoft.EntityFrameworkCore;
using TimeTracker.Data;
using TimeTracker.Models;
using TimeTracker.Models.Dto;

namespace TimeTracker.Services
{
    public class PeerReviewService : IPeerReviewService
    {
        private readonly ApplicationDBContext _context;

        public PeerReviewService(ApplicationDBContext context)
        {
            _context = context;
        }

    public async Task<PeerReviewWithAnswersDto?> GetPeerReviewByIdAsync(int id)
    {
        var peerReview = await _context.PeerReviews
            .Include(pr => pr.Reviewer) // Include the Reviewer
            .Include(pr => pr.Reviewee) // Include the Reviewee
            .FirstOrDefaultAsync(pr => pr.PeerReviewId == id);
        
        if (peerReview == null)
        {
            return null;
        }

        // Include the answers associated with the peer review
        var peerReviewAnswers = await _context.PeerReviewAnswers
            .Include(pra => pra.PeerReviewQuestion) // Include the question
            .Where(pra => pra.PeerReviewId == id)
            .ToListAsync();

        var result = new PeerReviewWithAnswersDto
        {
                RevieweeId = peerReview.RevieweeId,
                ReviewerId = peerReview.ReviewerId,
                PeerReviewAnswers = peerReviewAnswers
        };
        
        return result;
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