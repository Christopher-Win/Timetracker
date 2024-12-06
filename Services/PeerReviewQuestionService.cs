// *************************************************
// ***************** Written by: *******************
// ****************** Aayush P. ********************
// *************************************************

using Microsoft.EntityFrameworkCore; // Provides ORM functionality for interacting with the database
using TimeTracker.Data; // References the application's database context
using TimeTracker.Models; // References the PeerReviewQuestion model

namespace TimeTracker.Services
{
    // Implementation of the IPeerReviewQuestionService interface
    // Handles operations related to managing PeerReviewQuestions
    public class PeerReviewQuestionService : IPeerReviewQuestionService
    {
        private readonly ApplicationDBContext _context; // Database context for accessing and managing data

        // Constructor to inject the database context
        public PeerReviewQuestionService(ApplicationDBContext context)
        {
            _context = context; // Assigns the injected database context to a private field
        }

        // Retrieves all PeerReviewQuestions from the database
        public async Task<List<PeerReviewQuestion>> GetAllPeerReviewQuestionsAsync()
        {
            // Fetches all records from the PeerReviewQuestions table and returns them as a list
            return await _context.PeerReviewQuestions.ToListAsync();
        }

        // Retrieves a single PeerReviewQuestion by its unique ID
        public async Task<PeerReviewQuestion?> GetPeerReviewQuestionByIdAsync(int id)
        {
            // Uses FindAsync to locate a PeerReviewQuestion by its primary key (ID)
            // Returns the PeerReviewQuestion object if found, otherwise null
            return await _context.PeerReviewQuestions.FindAsync(id);
        }

        // Creates a new PeerReviewQuestion and saves it to the database
        public async Task CreatePeerReviewQuestionAsync(PeerReviewQuestion peerReviewQuestion)
        {
            // Adds the new PeerReviewQuestion to the context
            _context.PeerReviewQuestions.Add(peerReviewQuestion);
            // Persists changes to the database asynchronously
            await _context.SaveChangesAsync();
        }

        // Deletes a specific PeerReviewQuestion by its unique ID
        public async Task DeletePeerReviewQuestionAsync(int id)
        {
            // Finds the PeerReviewQuestion by its ID
            var question = await _context.PeerReviewQuestions.FindAsync(id);
            if (question != null) // Checks if the question exists
            {
                // Removes the question from the context
                _context.PeerReviewQuestions.Remove(question);
                // Persists the deletion to the database
                await _context.SaveChangesAsync();
            }
        }
    }
}