using TimeTracker.Models;

public interface IPeerReviewAnswerService
{
    Task CreatePeerReviewAnswerAsync(PeerReviewAnswer answer);
    Task<IEnumerable<PeerReviewAnswer>> GetAnswersByPeerReviewIdAsync(int peerReviewId);
    Task<PeerReviewQuestion?> GetPeerReviewQuestionByIdAsync(int questionId); // New method
}