using TimeTracker.Models;
using TimeTracker.Models.Dto;

namespace TimeTracker.Services
{
    public interface IPeerReviewService
    {
        Task CreatePeerReviewAsync(PeerReview peerReview);
        Task<PeerReview?> GetPeerReviewByIdAsync(int id);
        Task DeletePeerReviewAsync(int id);
        Task<PeerReview?> GetPeerReviewByReviewerAndRevieweeAsync(string reviewerId, string revieweeId);
    }
}