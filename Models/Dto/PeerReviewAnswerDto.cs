namespace TimeTracker.Models.Dto {
    public class PeerReviewAnswerDto
    {
        public int PeerReviewId { get; set; } // ID of the PeerReview this answer is for
        public int PeerReviewQuestionId { get; set; } // ID of the question this answer is for
        public int NumericalFeedback { get; set; }  // Numerical score for this question
        public required string WrittenFeedback { get; set; } // Written feedback for this question
    }
}