using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveVoicevoxSpeakerIdColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SpeakerId",
                table: "vocabulary_details");

            migrationBuilder.DropColumn(
                name: "SpeakerId",
                table: "sentences");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SpeakerId",
                table: "vocabulary_details",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SpeakerId",
                table: "sentences",
                type: "integer",
                nullable: true);
        }
    }
}
