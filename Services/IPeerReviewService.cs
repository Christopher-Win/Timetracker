using TimeTracker.Models;

namespace TimeTracker.Services
{
    public interface IPeerReviewService
    {
        Task CreatePeerReviewAsync(PeerReview peerReview);
        Task<PeerReview?> GetPeerReviewByIdAsync(int id);
        Task DeletePeerReviewAsync(int id);
    }
}