using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace translate.web.Migrations
{
    public partial class update4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Translations_ProjectDocuments_DocumentId",
                table: "Translations");

            migrationBuilder.DropIndex(
                name: "IX_Translations_DocumentId",
                table: "Translations");

            migrationBuilder.DropColumn(
                name: "DocumentId",
                table: "Translations");

            migrationBuilder.AddColumn<Guid>(
                name: "TranslationId",
                table: "ProjectDocuments",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDocuments_TranslationId",
                table: "ProjectDocuments",
                column: "TranslationId",
                unique: true,
                filter: "[TranslationId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectDocuments_Translations_TranslationId",
                table: "ProjectDocuments",
                column: "TranslationId",
                principalTable: "Translations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectDocuments_Translations_TranslationId",
                table: "ProjectDocuments");

            migrationBuilder.DropIndex(
                name: "IX_ProjectDocuments_TranslationId",
                table: "ProjectDocuments");

            migrationBuilder.DropColumn(
                name: "TranslationId",
                table: "ProjectDocuments");

            migrationBuilder.AddColumn<Guid>(
                name: "DocumentId",
                table: "Translations",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Translations_DocumentId",
                table: "Translations",
                column: "DocumentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Translations_ProjectDocuments_DocumentId",
                table: "Translations",
                column: "DocumentId",
                principalTable: "ProjectDocuments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
