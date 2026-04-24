using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddShadowingModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "shadowing_topics",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Level = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Visibility = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Public"),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Draft"),
                    IsOfficial = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    SentencesCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shadowing_topics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_shadowing_topics_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "shadowing_attempts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TopicId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SentenceId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AudioAssetId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Locale = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "ja-JP"),
                    RecognizedText = table.Column<string>(type: "text", nullable: true),
                    PronScore = table.Column<double>(type: "double precision", nullable: true),
                    AccuracyScore = table.Column<double>(type: "double precision", nullable: true),
                    FluencyScore = table.Column<double>(type: "double precision", nullable: true),
                    CompletenessScore = table.Column<double>(type: "double precision", nullable: true),
                    ProsodyScore = table.Column<double>(type: "double precision", nullable: true),
                    ErrorTypes = table.Column<string>(type: "text", nullable: true),
                    DurationMs = table.Column<int>(type: "integer", nullable: true),
                    RawResultJson = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shadowing_attempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_shadowing_attempts_MediaAssets_AudioAssetId",
                        column: x => x.AudioAssetId,
                        principalTable: "MediaAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_shadowing_attempts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_shadowing_attempts_sentences_SentenceId",
                        column: x => x.SentenceId,
                        principalTable: "sentences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_shadowing_attempts_shadowing_topics_TopicId",
                        column: x => x.TopicId,
                        principalTable: "shadowing_topics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "shadowing_topic_sentences",
                columns: table => new
                {
                    TopicId = table.Column<string>(type: "text", nullable: false),
                    SentenceId = table.Column<string>(type: "text", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shadowing_topic_sentences", x => new { x.TopicId, x.SentenceId });
                    table.ForeignKey(
                        name: "FK_shadowing_topic_sentences_sentences_SentenceId",
                        column: x => x.SentenceId,
                        principalTable: "sentences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_shadowing_topic_sentences_shadowing_topics_TopicId",
                        column: x => x.TopicId,
                        principalTable: "shadowing_topics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_shadowing_attempts_topic_created",
                table: "shadowing_attempts",
                columns: new[] { "TopicId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "idx_shadowing_attempts_user_created",
                table: "shadowing_attempts",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "idx_shadowing_attempts_user_sentence_created",
                table: "shadowing_attempts",
                columns: new[] { "UserId", "SentenceId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_shadowing_attempts_AudioAssetId",
                table: "shadowing_attempts",
                column: "AudioAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_shadowing_attempts_SentenceId",
                table: "shadowing_attempts",
                column: "SentenceId");

            migrationBuilder.CreateIndex(
                name: "idx_shadowing_topic_sentences_topic_position",
                table: "shadowing_topic_sentences",
                columns: new[] { "TopicId", "Position" });

            migrationBuilder.CreateIndex(
                name: "IX_shadowing_topic_sentences_SentenceId",
                table: "shadowing_topic_sentences",
                column: "SentenceId");

            migrationBuilder.CreateIndex(
                name: "idx_shadowing_topics_created_by",
                table: "shadowing_topics",
                columns: new[] { "CreatedBy", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "idx_shadowing_topics_visibility_status",
                table: "shadowing_topics",
                columns: new[] { "Visibility", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "shadowing_attempts");

            migrationBuilder.DropTable(
                name: "shadowing_topic_sentences");

            migrationBuilder.DropTable(
                name: "shadowing_topics");
        }
    }
}
