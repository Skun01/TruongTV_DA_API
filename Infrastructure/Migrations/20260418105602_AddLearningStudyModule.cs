using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLearningStudyModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "AnswerList",
                table: "card_sentences",
                type: "text[]",
                nullable: false,
                defaultValueSql: "'{}'::text[]");

            migrationBuilder.AddColumn<string>(
                name: "BlankWord",
                table: "card_sentences",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Hint",
                table: "card_sentences",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Position",
                table: "card_sentences",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "study_sessions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DeckId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Mode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SelectedFolderIds = table.Column<List<string>>(type: "text[]", nullable: false, defaultValueSql: "'{}'::text[]"),
                    CardIds = table.Column<List<string>>(type: "text[]", nullable: false, defaultValueSql: "'{}'::text[]"),
                    CompletedCardIds = table.Column<List<string>>(type: "text[]", nullable: false, defaultValueSql: "'{}'::text[]"),
                    CorrectCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IncorrectCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_study_sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_study_sessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_study_sessions_decks_DeckId",
                        column: x => x.DeckId,
                        principalTable: "decks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_card_progress",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "character varying(50)", nullable: false),
                    CardId = table.Column<string>(type: "text", nullable: false),
                    SrsLevel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Level1"),
                    NextReviewAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConsecutiveCorrect = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsMastered = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    LastSentenceId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_card_progress", x => new { x.UserId, x.CardId });
                    table.ForeignKey(
                        name: "FK_user_card_progress_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_card_progress_cards_CardId",
                        column: x => x.CardId,
                        principalTable: "cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_card_progress_sentences_LastSentenceId",
                        column: x => x.LastSentenceId,
                        principalTable: "sentences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "idx_card_sentences_card_position",
                table: "card_sentences",
                columns: new[] { "CardId", "Position" });

            migrationBuilder.CreateIndex(
                name: "idx_study_sessions_user_completed",
                table: "study_sessions",
                columns: new[] { "UserId", "CompletedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_study_sessions_DeckId",
                table: "study_sessions",
                column: "DeckId");

            migrationBuilder.CreateIndex(
                name: "idx_user_card_progress_user_next_review",
                table: "user_card_progress",
                columns: new[] { "UserId", "NextReviewAt" });

            migrationBuilder.CreateIndex(
                name: "IX_user_card_progress_CardId",
                table: "user_card_progress",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_user_card_progress_LastSentenceId",
                table: "user_card_progress",
                column: "LastSentenceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "study_sessions");

            migrationBuilder.DropTable(
                name: "user_card_progress");

            migrationBuilder.DropIndex(
                name: "idx_card_sentences_card_position",
                table: "card_sentences");

            migrationBuilder.DropColumn(
                name: "AnswerList",
                table: "card_sentences");

            migrationBuilder.DropColumn(
                name: "BlankWord",
                table: "card_sentences");

            migrationBuilder.DropColumn(
                name: "Hint",
                table: "card_sentences");

            migrationBuilder.DropColumn(
                name: "Position",
                table: "card_sentences");
        }
    }
}
