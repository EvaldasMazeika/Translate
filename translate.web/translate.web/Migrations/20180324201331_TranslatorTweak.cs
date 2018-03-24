using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace translate.web.Migrations
{
    public partial class TranslatorTweak : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TranslatorId",
                table: "Translations",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Translations_TranslatorId",
                table: "Translations",
                column: "TranslatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Translations_Users_TranslatorId",
                table: "Translations",
                column: "TranslatorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Translations_Users_TranslatorId",
                table: "Translations");

            migrationBuilder.DropIndex(
                name: "IX_Translations_TranslatorId",
                table: "Translations");

            migrationBuilder.DropColumn(
                name: "TranslatorId",
                table: "Translations");
        }
    }
}
