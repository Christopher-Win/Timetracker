using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeTracker.Migrations
{
    /// <inheritdoc />
    public partial class PeerReview_PeerReviewQuestion_PeerReviewAnswer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PeerReviewQuestions",
                columns: table => new
                {
                    PeerReviewQuestionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    QuestionText = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeerReviewQuestions", x => x.PeerReviewQuestionId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PeerReviews",
                columns: table => new
                {
                    PeerReviewId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ReviewerId = table.Column<int>(type: "int", nullable: false),
                    RevieweeId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeerReviews", x => x.PeerReviewId);
                    table.ForeignKey(
                        name: "FK_PeerReviews_Users_RevieweeId",
                        column: x => x.RevieweeId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PeerReviews_Users_ReviewerId",
                        column: x => x.ReviewerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PeerReviewAnswers",
                columns: table => new
                {
                    PeerReviewAnswerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PeerReviewId = table.Column<int>(type: "int", nullable: false),
                    PeerReviewQuestionId = table.Column<int>(type: "int", nullable: false),
                    NumericalFeedback = table.Column<int>(type: "int", nullable: false),
                    WrittenFeedback = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeerReviewAnswers", x => x.PeerReviewAnswerId);
                    table.ForeignKey(
                        name: "FK_PeerReviewAnswers_PeerReviewQuestions_PeerReviewQuestionId",
                        column: x => x.PeerReviewQuestionId,
                        principalTable: "PeerReviewQuestions",
                        principalColumn: "PeerReviewQuestionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PeerReviewAnswers_PeerReviews_PeerReviewId",
                        column: x => x.PeerReviewId,
                        principalTable: "PeerReviews",
                        principalColumn: "PeerReviewId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_PeerReviewAnswers_PeerReviewId",
                table: "PeerReviewAnswers",
                column: "PeerReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_PeerReviewAnswers_PeerReviewQuestionId",
                table: "PeerReviewAnswers",
                column: "PeerReviewQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_PeerReviews_RevieweeId",
                table: "PeerReviews",
                column: "RevieweeId");

            migrationBuilder.CreateIndex(
                name: "IX_PeerReviews_ReviewerId",
                table: "PeerReviews",
                column: "ReviewerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PeerReviewAnswers");

            migrationBuilder.DropTable(
                name: "PeerReviewQuestions");

            migrationBuilder.DropTable(
                name: "PeerReviews");
        }
    }
}
