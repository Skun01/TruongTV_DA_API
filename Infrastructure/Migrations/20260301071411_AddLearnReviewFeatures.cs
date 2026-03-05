using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLearnReviewFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CardProgresses",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    CardId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CardType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "New"),
                    SrsLevel = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CorrectStreak = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TotalReviews = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CorrectReviews = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    NextExampleIndex = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LearnedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextReviewAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CardProgresses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeckQueues",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    DeckId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeckQueues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeckQueues_Decks_DeckId",
                        column: x => x.DeckId,
                        principalTable: "Decks",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DeckQueues_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSettings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    DailyGoal = table.Column<int>(type: "int", nullable: false, defaultValue: 10),
                    BatchSize = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    CurrentStreak = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LongestStreak = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LastStudyDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSettings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReviewLogs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    ExampleSentenceId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CardProgressId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    UserAnswer = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsGhost = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReviewLogs_CardProgresses_CardProgressId",
                        column: x => x.CardProgressId,
                        principalTable: "CardProgresses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReviewLogs_ExampleSentences_ExampleSentenceId",
                        column: x => x.ExampleSentenceId,
                        principalTable: "ExampleSentences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ReviewLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CardProgresses_UserId_CardId_CardType",
                table: "CardProgresses",
                columns: new[] { "UserId", "CardId", "CardType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CardProgresses_UserId_NextReviewAt",
                table: "CardProgresses",
                columns: new[] { "UserId", "NextReviewAt" });

            migrationBuilder.CreateIndex(
                name: "IX_DeckQueues_DeckId",
                table: "DeckQueues",
                column: "DeckId");

            migrationBuilder.CreateIndex(
                name: "IX_DeckQueues_UserId_DeckId",
                table: "DeckQueues",
                columns: new[] { "UserId", "DeckId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReviewLogs_CardProgressId",
                table: "ReviewLogs",
                column: "CardProgressId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewLogs_ExampleSentenceId",
                table: "ReviewLogs",
                column: "ExampleSentenceId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewLogs_UserId",
                table: "ReviewLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSettings_UserId",
                table: "UserSettings",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeckQueues");

            migrationBuilder.DropTable(
                name: "ReviewLogs");

            migrationBuilder.DropTable(
                name: "UserSettings");

            migrationBuilder.DropTable(
                name: "CardProgresses");
        }
    }
}
