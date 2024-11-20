using Microsoft.EntityFrameworkCore;
using TimeTracker.Data;
using TimeTracker.Models;

public class PeerReviewAnswerService : IPeerReviewAnswerService
{
    private readonly ApplicationDBContext _context;

    public PeerReviewAnswerService(ApplicationDBContext context)
    {
        _context = context;
    }

    public async Task CreatePeerReviewAnswerAsync(PeerReviewAnswer answer)
    {
        _context.PeerReviewAnswers.Add(answer);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<PeerReviewAnswer>> GetAnswersByPeerReviewIdAsync(int peerReviewId)
    {
        return await _context.PeerReviewAnswers
            .Where(a => a.PeerReviewId == peerReviewId)
            .Include(a => a.PeerReviewQuestion) // Include question details if needed
            .ToListAsync();
    }

    public async Task<PeerReviewQuestion?> GetPeerReviewQuestionByIdAsync(int questionId)
    {
        return await _context.PeerReviewQuestions.FindAsync(questionId); // Retrieve the question by ID
    }

    public async Task<PeerReview?> GetPeerReviewByReviewerAndRevieweeAsync(string reviewerId, string revieweeId)
    {
        return await _context.PeerReviews
            .Include(pr => pr.Reviewer)
            .Include(pr => pr.Reviewee)
            .Where(pr => pr.ReviewerId == reviewerId && pr.RevieweeId == revieweeId)
            .OrderByDescending(pr => pr.SubmittedAt) // Or use pr.PeerReviewId
            .FirstOrDefaultAsync();
    }
    
}