using System;
using System.Collections.Generic;
using Domain.Entities;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateConversationTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "conversation_sessions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Scenario = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CustomScenario = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Level = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "N5"),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TotalMessages = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    UserMessagesCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Score = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conversation_sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_conversation_sessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "conversation_messages",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ConversationId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Sender = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "User"),
                    Text = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Suggestions = table.Column<List<string>>(type: "jsonb", nullable: true),
                    NewVocabulary = table.Column<List<ExtractedVocabulary>>(type: "jsonb", nullable: true),
                    GrammarPoints = table.Column<List<string>>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conversation_messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_conversation_messages_conversation_sessions_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "conversation_sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "extracted_vocabularies",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    MessageId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Word = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Reading = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Meaning = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Example = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    JlptLevel = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "N5"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_extracted_vocabularies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_extracted_vocabularies_conversation_messages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "conversation_messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_conversation_messages_conversation",
                table: "conversation_messages",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "idx_conversation_sessions_status",
                table: "conversation_sessions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "idx_conversation_sessions_user",
                table: "conversation_sessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "idx_extracted_vocabularies_message",
                table: "extracted_vocabularies",
                column: "MessageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "extracted_vocabularies");

            migrationBuilder.DropTable(
                name: "conversation_messages");

            migrationBuilder.DropTable(
                name: "conversation_sessions");
        }
    }
}
