// *************************************************
// ***************** Written by: *******************
// ****************** Aayush P. ********************
// *************************************************

// References the PeerReview model and associated DTOs for managing peer review operations
using TimeTracker.Models;
using TimeTracker.Models.Dto;

namespace TimeTracker.Services
{
    // Interface defining the contract for PeerReview-related operations
    public interface IPeerReviewService
    {
        // Creates a new PeerReview and saves it to the database.
        // Takes a PeerReview object as input and performs the creation asynchronously.
        Task CreatePeerReviewAsync(PeerReview peerReview);

        // Retrieves a specific PeerReview by its unique ID.
        // Takes the peer review ID as input and returns the PeerReview object if found, otherwise null.
        Task<PeerReview?> GetPeerReviewByIdAsync(int id);

        // Deletes a PeerReview by its unique ID.
        // Takes the peer review ID as input and deletes the associated PeerReview asynchronously.
        Task DeletePeerReviewAsync(int id);

        // Retrieves a PeerReview for a specific reviewer-reviewee pair.
        // Takes the reviewer's unique ID and the reviewee's unique ID as inputs.
        // Returns the PeerReview object if found, otherwise null.
        Task<PeerReview?> GetPeerReviewByReviewerAndRevieweeAsync(string reviewerId, string revieweeId);
    }
}