// References application models like PeerReviewAnswer, PeerReviewQuestion, and PeerReview
using TimeTracker.Models;

// Interface defining the contract for PeerReviewAnswer-related operations
public interface IPeerReviewAnswerService
{
    // Creates a new PeerReviewAnswer and saves it to the database.
    Task CreatePeerReviewAnswerAsync(PeerReviewAnswer answer);

    // Retrieves all PeerReviewAnswers associated with a specific PeerReview.
    // Takes the unique ID of the PeerReview as input and returns a collection of answers.
    Task<IEnumerable<PeerReviewAnswer>> GetAnswersByPeerReviewIdAsync(int peerReviewId);

    // Retrieves a PeerReviewQuestion by its unique ID.
    // Takes the question ID as input and returns the PeerReviewQuestion object if found, otherwise null.
    Task<PeerReviewQuestion?> GetPeerReviewQuestionByIdAsync(int questionId);

    // Retrieves all PeerReviews for a specific reviewer-reviewee pair.
    // Takes the unique IDs of the reviewer and reviewee as input and returns a collection of PeerReviews.
    Task<IEnumerable<PeerReview>> GetPeerReviewsByReviewerAndRevieweeAsync(string reviewerId, string revieweeId);
}