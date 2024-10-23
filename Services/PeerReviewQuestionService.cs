using Microsoft.EntityFrameworkCore;
using TimeTracker.Data;
using TimeTracker.Models;

namespace TimeTracker.Services
{
    public class PeerReviewQuestionService : IPeerReviewQuestionService
    {
        private readonly ApplicationDBContext _context;

        public PeerReviewQuestionService(ApplicationDBContext context)
        {
            _context = context;
        }

        // Get all peer review questions
        public async Task<List<PeerReviewQuestion>> GetAllPeerReviewQuestionsAsync()
        {
            return await _context.PeerReviewQuestions.ToListAsync();
        }

        // Get a single peer review question by ID
        public async Task<PeerReviewQuestion?> GetPeerReviewQuestionByIdAsync(int id)
        {
            return await _context.PeerReviewQuestions.FindAsync(id);
        }

        // Create a new peer review question
        public async Task CreatePeerReviewQuestionAsync(PeerReviewQuestion peerReviewQuestion)
        {
            _context.PeerReviewQuestions.Add(peerReviewQuestion);
            await _context.SaveChangesAsync();
        }
    }
}