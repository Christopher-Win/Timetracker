namespace TimeTracker.Models{
    public class PeerReviewAnswer
    {
        public int PeerReviewAnswerId { get; set; } // The answer ID

        // Foreign key to PeerReview and PeerReviewQuestion
        public int PeerReviewId { get; set; } // The review ID for this answer
        public required PeerReview PeerReview { get; set; } // Navigation property to the PeerReview

        public int PeerReviewQuestionId { get; set; } // The question ID for this answer
        public required PeerReviewQuestion PeerReviewQuestion { get; set; } // Navigation property to the PeerReviewQuestion

        // Answer details
        public int NumericalFeedback { get; set; }  // Numerical score for this specific question
        public required string WrittenFeedback { get; set; } // Written feedback for this specific question
    }
}