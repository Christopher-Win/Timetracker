// References the PeerReviewQuestion model for managing peer review questions
using TimeTracker.Models;

namespace TimeTracker.Services
{
    // Interface defining the contract for PeerReviewQuestion-related operations
    public interface IPeerReviewQuestionService
    {
        // Retrieves all PeerReviewQuestions from the database.
        // Returns a list of PeerReviewQuestion objects asynchronously.
        Task<List<PeerReviewQuestion>> GetAllPeerReviewQuestionsAsync();

        // Retrieves a specific PeerReviewQuestion by its unique ID.
        // Takes the question ID as input and returns the PeerReviewQuestion object if found, otherwise null.
        Task<PeerReviewQuestion?> GetPeerReviewQuestionByIdAsync(int id);

        // Creates a new PeerReviewQuestion and saves it to the database.
        // Takes a PeerReviewQuestion object as input and performs the creation asynchronously.
        Task CreatePeerReviewQuestionAsync(PeerReviewQuestion peerReviewQuestion);

        // Deletes a PeerReviewQuestion by its unique ID.
        // Takes the question ID as input and deletes the associated PeerReviewQuestion asynchronously.
        Task DeletePeerReviewQuestionAsync(int id);
    }
}