using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGrammarModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "grammar_details",
                columns: table => new
                {
                    CardId = table.Column<string>(type: "text", nullable: false),
                    Explanation = table.Column<string>(type: "text", nullable: true),
                    Caution = table.Column<string>(type: "text", nullable: true),
                    Register = table.Column<int>(type: "integer", nullable: true),
                    AlternateForms = table.Column<List<string>>(type: "text[]", nullable: false),
                    structures = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grammar_details", x => x.CardId);
                    table.ForeignKey(
                        name: "FK_grammar_details_cards_CardId",
                        column: x => x.CardId,
                        principalTable: "cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "grammar_relations",
                columns: table => new
                {
                    GrammarId = table.Column<string>(type: "text", nullable: false),
                    RelatedId = table.Column<string>(type: "text", nullable: false),
                    RelationType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grammar_relations", x => new { x.GrammarId, x.RelatedId, x.RelationType });
                    table.ForeignKey(
                        name: "FK_grammar_relations_cards_GrammarId",
                        column: x => x.GrammarId,
                        principalTable: "cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_grammar_relations_cards_RelatedId",
                        column: x => x.RelatedId,
                        principalTable: "cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "grammar_resources",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    CardId = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grammar_resources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_grammar_resources_cards_CardId",
                        column: x => x.CardId,
                        principalTable: "cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_grammar_relations_RelatedId",
                table: "grammar_relations",
                column: "RelatedId");

            migrationBuilder.CreateIndex(
                name: "IX_grammar_resources_CardId",
                table: "grammar_resources",
                column: "CardId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "grammar_details");

            migrationBuilder.DropTable(
                name: "grammar_relations");

            migrationBuilder.DropTable(
                name: "grammar_resources");
        }
    }
}
