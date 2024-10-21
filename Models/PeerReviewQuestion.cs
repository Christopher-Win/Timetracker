namespace TimeTracker.Models{
    public class PeerReviewQuestion
    {
        public int PeerReviewQuestionId { get; set; } // The question ID
        public required string QuestionText { get; set; }  // The question text
    }
}