using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLearningSrsLevelsAndDueCards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE user_card_progress
                SET "SrsLevel" = CASE
                    WHEN "SrsLevel" = 'Level1' THEN 'level_1'
                    WHEN "SrsLevel" = 'Level2' THEN 'level_2'
                    WHEN "SrsLevel" = 'Level3' THEN 'level_3'
                    WHEN "SrsLevel" = 'Level4' THEN 'level_4'
                    WHEN "SrsLevel" = 'Level5' AND "IsMastered" = false THEN 'level_5'
                    WHEN "SrsLevel" = 'Level5' AND "IsMastered" = true THEN 'level_12'
                    ELSE "SrsLevel"
                END;
                """);

            migrationBuilder.Sql("""
                UPDATE user_card_progress
                SET "NextReviewAt" = now() + interval '100 years'
                WHERE "SrsLevel" = 'level_12';
                """);

            migrationBuilder.AlterColumn<string>(
                name: "SrsLevel",
                table: "user_card_progress",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "level_1",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Level1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SrsLevel",
                table: "user_card_progress",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Level1",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "level_1");

            migrationBuilder.Sql("""
                UPDATE user_card_progress
                SET "SrsLevel" = CASE
                    WHEN "SrsLevel" = 'level_1' THEN 'Level1'
                    WHEN "SrsLevel" = 'level_2' THEN 'Level2'
                    WHEN "SrsLevel" = 'level_3' THEN 'Level3'
                    WHEN "SrsLevel" = 'level_4' THEN 'Level4'
                    WHEN "SrsLevel" = 'level_5' THEN 'Level5'
                    WHEN "SrsLevel" = 'level_12' THEN 'Level5'
                    ELSE "SrsLevel"
                END;
                """);
        }
    }
}
