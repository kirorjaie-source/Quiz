using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_gameSessions_quizzes_QuizId",
                table: "gameSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_playerAnswers_players_PlayerId",
                table: "playerAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_quizzes_users_CreatorId",
                table: "quizzes");

            migrationBuilder.CreateIndex(
                name: "IX_gameSessions_HostId",
                table: "gameSessions",
                column: "HostId");

            migrationBuilder.AddForeignKey(
                name: "FK_gameSessions_quizzes_QuizId",
                table: "gameSessions",
                column: "QuizId",
                principalTable: "quizzes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_gameSessions_users_HostId",
                table: "gameSessions",
                column: "HostId",
                principalTable: "users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_playerAnswers_players_PlayerId",
                table: "playerAnswers",
                column: "PlayerId",
                principalTable: "players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_quizzes_users_CreatorId",
                table: "quizzes",
                column: "CreatorId",
                principalTable: "users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_gameSessions_quizzes_QuizId",
                table: "gameSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_gameSessions_users_HostId",
                table: "gameSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_playerAnswers_players_PlayerId",
                table: "playerAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_quizzes_users_CreatorId",
                table: "quizzes");

            migrationBuilder.DropIndex(
                name: "IX_gameSessions_HostId",
                table: "gameSessions");

            migrationBuilder.AddForeignKey(
                name: "FK_gameSessions_quizzes_QuizId",
                table: "gameSessions",
                column: "QuizId",
                principalTable: "quizzes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_playerAnswers_players_PlayerId",
                table: "playerAnswers",
                column: "PlayerId",
                principalTable: "players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_quizzes_users_CreatorId",
                table: "quizzes",
                column: "CreatorId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
