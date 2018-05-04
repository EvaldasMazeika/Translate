using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace translate.web.Migrations
{
    public partial class update6 : Migration
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

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "Translations");

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                table: "Translations",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Translations_ProjectId",
                table: "Translations",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Translations_Projects_ProjectId",
                table: "Translations",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Translations_Projects_ProjectId",
                table: "Translations");

            migrationBuilder.DropIndex(
                name: "IX_Translations_ProjectId",
                table: "Translations");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "Translations");

            migrationBuilder.AddColumn<Guid>(
                name: "DocumentId",
                table: "Translations",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "Translations",
                nullable: true);

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
                onDelete: ReferentialAction.Restrict);
        }
    }
}
