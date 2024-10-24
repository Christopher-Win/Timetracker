// File: Data/ApplicationDBContext.cs

using Microsoft.EntityFrameworkCore;
using TimeTracker.Models;  // Make sure to adjust this namespace for your project

namespace TimeTracker.Data
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }   // Example model
        public DbSet<TimeLog> TimeLogs { get; set; }
        public DbSet<TimeLogEntry> TimeLogEntries { get; set; }
        public DbSet<PeerReview> PeerReviews { get; set; } // Peer reviews
        public DbSet<PeerReviewQuestion> PeerReviewQuestions { get; set; }  // Predefined questions
        public DbSet<PeerReviewAnswer> PeerReviewAnswers { get; set; }  // Answers for the questions

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // One-to-many relationship between User and TimeLog
            modelBuilder.Entity<User>()
                .HasMany(u => u.TimeLogs)
                .WithOne(t => t.User)
                .HasForeignKey(t => t.UserId);

            // One-to-many relationship between TimeLog and TimeLogEntry
            modelBuilder.Entity<TimeLog>()
                .HasMany(t => t.TimeLogEntries)
                .WithOne(e => e.TimeLog)
                .HasForeignKey(e => e.TimeLogId);

            // Peer Review relationships
            modelBuilder.Entity<PeerReview>()
                .HasOne(pr => pr.Reviewer)
                .WithMany(u => u.ReviewsGiven)
                .HasForeignKey(pr => pr.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PeerReview>()
                .HasOne(pr => pr.Reviewee)
                .WithMany(u => u.ReviewsReceived)
                .HasForeignKey(pr => pr.RevieweeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relationship between PeerReview and PeerReviewAnswer
            modelBuilder.Entity<PeerReviewAnswer>()
                .HasOne(pra => pra.PeerReview)
                .WithMany(pr => pr.PeerReviewAnswers)
                .HasForeignKey(pra => pra.PeerReviewId);

            // Relationship between PeerReviewAnswer and PeerReviewQuestion
            modelBuilder.Entity<PeerReviewAnswer>()
                .HasOne(pra => pra.PeerReviewQuestion)
                .WithMany()
                .HasForeignKey(pra => pra.PeerReviewQuestionId);
           
        }
    }
}