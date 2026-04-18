using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLearningSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FlashcardBack",
                table: "study_sessions",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Summary");

            migrationBuilder.AddColumn<string>(
                name: "FlashcardFront",
                table: "study_sessions",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Title");

            migrationBuilder.AddColumn<string>(
                name: "MultipleChoiceQuestion",
                table: "study_sessions",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "TitleToSummary");

            migrationBuilder.AddColumn<bool>(
                name: "ShuffleOptions",
                table: "study_sessions",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateTable(
                name: "user_learning_settings",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FlashcardFront = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Title"),
                    FlashcardBack = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Summary"),
                    MultipleChoiceQuestion = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "TitleToSummary"),
                    ShuffleOptions = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_learning_settings", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_user_learning_settings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_learning_settings");

            migrationBuilder.DropColumn(
                name: "FlashcardBack",
                table: "study_sessions");

            migrationBuilder.DropColumn(
                name: "FlashcardFront",
                table: "study_sessions");

            migrationBuilder.DropColumn(
                name: "MultipleChoiceQuestion",
                table: "study_sessions");

            migrationBuilder.DropColumn(
                name: "ShuffleOptions",
                table: "study_sessions");
        }
    }
}
