using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDeckModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "deck_types",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_deck_types", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "decks",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ForkedFromId = table.Column<string>(type: "text", nullable: true),
                    TypeId = table.Column<string>(type: "text", nullable: true),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false, defaultValue: ""),
                    CoverImageUrl = table.Column<string>(type: "text", nullable: true),
                    Visibility = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Public"),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Draft"),
                    IsOfficial = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CardsCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    FoldersCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_decks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_decks_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_decks_deck_types_TypeId",
                        column: x => x.TypeId,
                        principalTable: "deck_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_decks_decks_ForkedFromId",
                        column: x => x.ForkedFromId,
                        principalTable: "decks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "deck_bookmarks",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DeckId = table.Column<string>(type: "text", nullable: false),
                    SavedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_deck_bookmarks", x => new { x.UserId, x.DeckId });
                    table.ForeignKey(
                        name: "FK_deck_bookmarks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_deck_bookmarks_decks_DeckId",
                        column: x => x.DeckId,
                        principalTable: "decks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "deck_folders",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    DeckId = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false, defaultValue: ""),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    CardsCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_deck_folders", x => x.Id);
                    table.UniqueConstraint("AK_deck_folders_DeckId_Id", x => new { x.DeckId, x.Id });
                    table.ForeignKey(
                        name: "FK_deck_folders_decks_DeckId",
                        column: x => x.DeckId,
                        principalTable: "decks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "folder_cards",
                columns: table => new
                {
                    FolderId = table.Column<string>(type: "text", nullable: false),
                    CardId = table.Column<string>(type: "text", nullable: false),
                    DeckId = table.Column<string>(type: "text", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_folder_cards", x => new { x.FolderId, x.CardId });
                    table.ForeignKey(
                        name: "FK_folder_cards_cards_CardId",
                        column: x => x.CardId,
                        principalTable: "cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_folder_cards_deck_folders_DeckId_FolderId",
                        columns: x => new { x.DeckId, x.FolderId },
                        principalTable: "deck_folders",
                        principalColumns: new[] { "DeckId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_folder_cards_decks_DeckId",
                        column: x => x.DeckId,
                        principalTable: "decks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_deck_bookmarks_deck_id",
                table: "deck_bookmarks",
                column: "DeckId");

            migrationBuilder.CreateIndex(
                name: "idx_deck_folders_deck_position",
                table: "deck_folders",
                columns: new[] { "DeckId", "Position" });

            migrationBuilder.CreateIndex(
                name: "IX_deck_types_Name",
                table: "deck_types",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_decks_forked_from",
                table: "decks",
                column: "ForkedFromId");

            migrationBuilder.CreateIndex(
                name: "idx_decks_owner_created_at",
                table: "decks",
                columns: new[] { "CreatedBy", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "idx_decks_type_id",
                table: "decks",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "idx_decks_visibility_status",
                table: "decks",
                columns: new[] { "Visibility", "Status" });

            migrationBuilder.CreateIndex(
                name: "idx_folder_cards_card_id",
                table: "folder_cards",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "idx_folder_cards_folder_position",
                table: "folder_cards",
                columns: new[] { "FolderId", "Position" });

            migrationBuilder.CreateIndex(
                name: "IX_folder_cards_DeckId_FolderId",
                table: "folder_cards",
                columns: new[] { "DeckId", "FolderId" });

            migrationBuilder.CreateIndex(
                name: "uq_folder_cards_deck_card",
                table: "folder_cards",
                columns: new[] { "DeckId", "CardId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "deck_bookmarks");

            migrationBuilder.DropTable(
                name: "folder_cards");

            migrationBuilder.DropTable(
                name: "deck_folders");

            migrationBuilder.DropTable(
                name: "decks");

            migrationBuilder.DropTable(
                name: "deck_types");
        }
    }
}
