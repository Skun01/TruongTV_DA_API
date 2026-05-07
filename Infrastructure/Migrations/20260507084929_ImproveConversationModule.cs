using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ImproveConversationModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Feedback",
                table: "conversation_sessions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FeedbackGeneratedAt",
                table: "conversation_sessions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResultModel",
                table: "conversation_sessions",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResultPromptVersion",
                table: "conversation_sessions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.Sql(
                """
                INSERT INTO extracted_vocabularies ("Id", "MessageId", "Word", "Reading", "Meaning", "Example", "JlptLevel", "CreatedAt")
                SELECT
                    md5(cm."Id" || COALESCE(vocab.item ->> 'word', '') || vocab.ordinality::text),
                    cm."Id",
                    COALESCE(vocab.item ->> 'word', ''),
                    COALESCE(vocab.item ->> 'reading', ''),
                    COALESCE(vocab.item ->> 'meaning', ''),
                    NULLIF(COALESCE(vocab.item ->> 'example', ''), ''),
                    COALESCE(NULLIF(vocab.item ->> 'jlptLevel', ''), 'N5'),
                    COALESCE(cm."CreatedAt", now())
                FROM conversation_messages cm
                CROSS JOIN LATERAL jsonb_array_elements(COALESCE(cm."NewVocabulary", '[]'::jsonb)) WITH ORDINALITY AS vocab(item, ordinality)
                WHERE COALESCE(vocab.item ->> 'word', '') <> ''
                  AND NOT EXISTS (
                      SELECT 1
                      FROM extracted_vocabularies ev
                      WHERE ev."MessageId" = cm."Id"
                        AND lower(ev."Word") = lower(COALESCE(vocab.item ->> 'word', ''))
                  );
                """);

            migrationBuilder.DropColumn(
                name: "NewVocabulary",
                table: "conversation_messages");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Feedback",
                table: "conversation_sessions");

            migrationBuilder.DropColumn(
                name: "FeedbackGeneratedAt",
                table: "conversation_sessions");

            migrationBuilder.DropColumn(
                name: "ResultModel",
                table: "conversation_sessions");

            migrationBuilder.DropColumn(
                name: "ResultPromptVersion",
                table: "conversation_sessions");

            migrationBuilder.AddColumn<string>(
                name: "NewVocabulary",
                table: "conversation_messages",
                type: "jsonb",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE conversation_messages cm
                SET "NewVocabulary" = vocab.data
                FROM (
                    SELECT
                        ev."MessageId",
                        jsonb_agg(
                            jsonb_build_object(
                                'word', ev."Word",
                                'reading', ev."Reading",
                                'meaning', ev."Meaning",
                                'example', COALESCE(ev."Example", ''),
                                'jlptLevel', ev."JlptLevel"
                            )
                            ORDER BY ev."CreatedAt"
                        ) AS data
                    FROM extracted_vocabularies ev
                    GROUP BY ev."MessageId"
                ) vocab
                WHERE cm."Id" = vocab."MessageId";
                """);
        }
    }
}
