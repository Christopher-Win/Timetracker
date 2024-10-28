using TimeTracker.Models;

namespace TimeTracker.Services
{
    public interface IPeerReviewQuestionService
    {
        Task<List<PeerReviewQuestion>> GetAllPeerReviewQuestionsAsync();
        Task<PeerReviewQuestion?> GetPeerReviewQuestionByIdAsync(int id);
        Task CreatePeerReviewQuestionAsync(PeerReviewQuestion peerReviewQuestion);
        Task DeletePeerReviewQuestionAsync (int id);
    }
}