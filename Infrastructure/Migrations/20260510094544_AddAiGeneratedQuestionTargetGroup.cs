using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAiGeneratedQuestionTargetGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "QuestionGroupId",
                table: "ai_generated_questions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ai_generated_questions_QuestionGroupId",
                table: "ai_generated_questions",
                column: "QuestionGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_ai_generated_questions_question_groups_QuestionGroupId",
                table: "ai_generated_questions",
                column: "QuestionGroupId",
                principalTable: "question_groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ai_generated_questions_question_groups_QuestionGroupId",
                table: "ai_generated_questions");

            migrationBuilder.DropIndex(
                name: "IX_ai_generated_questions_QuestionGroupId",
                table: "ai_generated_questions");

            migrationBuilder.DropColumn(
                name: "QuestionGroupId",
                table: "ai_generated_questions");
        }
    }
}
