using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddKanjiModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "kanji_details",
                columns: table => new
                {
                    CardId = table.Column<string>(type: "text", nullable: false),
                    Kanji = table.Column<string>(type: "text", nullable: false),
                    StrokeCount = table.Column<int>(type: "integer", nullable: false),
                    StrokeOrderUrl = table.Column<string>(type: "text", nullable: true),
                    Onyomi = table.Column<List<string>>(type: "text[]", nullable: false),
                    Kunyomi = table.Column<List<string>>(type: "text[]", nullable: false),
                    HanViet = table.Column<string>(type: "text", nullable: true),
                    MeaningVi = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kanji_details", x => x.CardId);
                    table.ForeignKey(
                        name: "FK_kanji_details_cards_CardId",
                        column: x => x.CardId,
                        principalTable: "cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "radical_details",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Character = table.Column<string>(type: "text", nullable: false),
                    MeaningVi = table.Column<string>(type: "text", nullable: false),
                    KanjiCardId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_radical_details", x => x.Id);
                    table.ForeignKey(
                        name: "FK_radical_details_cards_KanjiCardId",
                        column: x => x.KanjiCardId,
                        principalTable: "cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "kanji_radicals",
                columns: table => new
                {
                    KanjiId = table.Column<string>(type: "text", nullable: false),
                    RadicalId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kanji_radicals", x => new { x.KanjiId, x.RadicalId });
                    table.ForeignKey(
                        name: "FK_kanji_radicals_cards_KanjiId",
                        column: x => x.KanjiId,
                        principalTable: "cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_kanji_radicals_radical_details_RadicalId",
                        column: x => x.RadicalId,
                        principalTable: "radical_details",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_kanji_details_Kanji",
                table: "kanji_details",
                column: "Kanji",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_kanji_details_StrokeCount",
                table: "kanji_details",
                column: "StrokeCount");

            migrationBuilder.CreateIndex(
                name: "IX_kanji_radicals_RadicalId",
                table: "kanji_radicals",
                column: "RadicalId");

            migrationBuilder.CreateIndex(
                name: "IX_radical_details_Character",
                table: "radical_details",
                column: "Character",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_radical_details_KanjiCardId",
                table: "radical_details",
                column: "KanjiCardId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "kanji_details");

            migrationBuilder.DropTable(
                name: "kanji_radicals");

            migrationBuilder.DropTable(
                name: "radical_details");
        }
    }
}
