// *************************************************
// ***************** Written by: *******************
// ****************** Aayush P. ********************
// *************************************************

namespace TimeTracker.Models.Dto
{
    public class PeerReviewSubmissionDto
    {
        public required string RevieweeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string PeerReviewAnswersJson { get; set; } = string.Empty; // Accept answers as JSON string
    }
}