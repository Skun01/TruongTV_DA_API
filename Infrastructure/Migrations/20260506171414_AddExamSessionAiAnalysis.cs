using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExamSessionAiAnalysis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "exam_session_ai_analyses",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ExamSessionId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PromptVersion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Completed"),
                    InputHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    OutputJson = table.Column<string>(type: "text", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    LatencyMs = table.Column<int>(type: "integer", nullable: true),
                    TriggerType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "AutoGenerate"),
                    TriggerReason = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_session_ai_analyses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_exam_session_ai_analyses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exam_session_ai_analyses_exam_sessions_ExamSessionId",
                        column: x => x.ExamSessionId,
                        principalTable: "exam_sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_exam_session_ai_analyses_session_prompt_hash",
                table: "exam_session_ai_analyses",
                columns: new[] { "ExamSessionId", "PromptVersion", "InputHash" });

            migrationBuilder.CreateIndex(
                name: "idx_exam_session_ai_analyses_user_created",
                table: "exam_session_ai_analyses",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "idx_exam_session_ai_analyses_user_trigger_created",
                table: "exam_session_ai_analyses",
                columns: new[] { "UserId", "TriggerType", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "exam_session_ai_analyses");
        }
    }
}
