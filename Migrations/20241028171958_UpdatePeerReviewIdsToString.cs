using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeTracker.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePeerReviewIdsToString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PeerReviews_Users_RevieweeId",
                table: "PeerReviews");

            migrationBuilder.DropForeignKey(
                name: "FK_PeerReviews_Users_ReviewerId",
                table: "PeerReviews");

            migrationBuilder.AlterColumn<string>(
                name: "ReviewerId",
                table: "PeerReviews",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "RevieweeId",
                table: "PeerReviews",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Users_NetID",
                table: "Users",
                column: "NetID");

            migrationBuilder.AddForeignKey(
                name: "FK_PeerReviews_Users_RevieweeId",
                table: "PeerReviews",
                column: "RevieweeId",
                principalTable: "Users",
                principalColumn: "NetID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PeerReviews_Users_ReviewerId",
                table: "PeerReviews",
                column: "ReviewerId",
                principalTable: "Users",
                principalColumn: "NetID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PeerReviews_Users_RevieweeId",
                table: "PeerReviews");

            migrationBuilder.DropForeignKey(
                name: "FK_PeerReviews_Users_ReviewerId",
                table: "PeerReviews");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Users_NetID",
                table: "Users");

            migrationBuilder.AlterColumn<int>(
                name: "ReviewerId",
                table: "PeerReviews",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "RevieweeId",
                table: "PeerReviews",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_PeerReviews_Users_RevieweeId",
                table: "PeerReviews",
                column: "RevieweeId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PeerReviews_Users_ReviewerId",
                table: "PeerReviews",
                column: "ReviewerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
