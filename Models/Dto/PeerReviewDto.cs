namespace TimeTracker.Models.Dto {
    public class PeerReviewSubmissionDto
    {
        public required string ReviewerId { get; set; }
        public required string RevieweeId { get; set; } // Only the reviewee ID is required from the client
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ICollection<PeerReviewAnswer> PeerReviewAnswers { get; set; } = new List<PeerReviewAnswer>(); // Can be left empty if answers are not required
    }

    public class PeerReviewWithAnswersDto{
        public required string RevieweeId{ get; set; }
        public required string ReviewerId{ get; set; }
        public DateTime StartDate { get; set;}
        public DateTime EndDate { get; set;}
        public ICollection<PeerReviewAnswer> PeerReviewAnswers { get; set; } = new List<PeerReviewAnswer>();

    }
}