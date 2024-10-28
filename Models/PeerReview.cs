namespace TimeTracker.Models{
    public class PeerReview
    {
        public int PeerReviewId { get; set; } // primary key for the review

        // Foreign keys to identify the reviewer and reviewee (both are Users)
        public required string ReviewerId { get; set; } // The person(netID) giving the review
        public required User Reviewer { get; set; } // navigation property to the reviewer

        public required string RevieweeId { get; set; } // The person(netID) being reviewed
        public required User Reviewee { get; set; } // navigation property to the reviewee

        // Time window for submission
        public DateTime StartDate { get; set; } // The earliest date the review can be submitted
        public DateTime EndDate { get; set; } // The latest date the review can be submitted

        // Submission date
        public DateTime SubmittedAt { get; set; } = DateTime.Now;

        // Collection of answers for each question in this review
        public required ICollection<PeerReviewAnswer> PeerReviewAnswers { get; set; }
    }
}