using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AllowGlobalReviewSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_study_sessions_decks_DeckId",
                table: "study_sessions");

            migrationBuilder.AlterColumn<string>(
                name: "DeckId",
                table: "study_sessions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddForeignKey(
                name: "FK_study_sessions_decks_DeckId",
                table: "study_sessions",
                column: "DeckId",
                principalTable: "decks",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_study_sessions_decks_DeckId",
                table: "study_sessions");

            migrationBuilder.Sql("""
                DELETE FROM study_sessions
                WHERE "DeckId" IS NULL;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "DeckId",
                table: "study_sessions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_study_sessions_decks_DeckId",
                table: "study_sessions",
                column: "DeckId",
                principalTable: "decks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
