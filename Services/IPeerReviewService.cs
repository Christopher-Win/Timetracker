using TimeTracker.Models;
using TimeTracker.Models.Dto;

namespace TimeTracker.Services
{
    public interface IPeerReviewService
    {
        Task CreatePeerReviewAsync(PeerReview peerReview);
        Task<PeerReviewWithAnswersDto> GetPeerReviewByIdAsync(int id); // Change the return type to PeerReviewWithAnswersDto //Chris N
        Task DeletePeerReviewAsync(int id);
    }
}