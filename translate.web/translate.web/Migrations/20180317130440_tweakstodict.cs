using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace translate.web.Migrations
{
    public partial class tweakstodict : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Value",
                table: "TranslationDictionarys",
                newName: "NewValue");

            migrationBuilder.AddColumn<string>(
                name: "GivenValue",
                table: "TranslationDictionarys",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GivenValue",
                table: "TranslationDictionarys");

            migrationBuilder.RenameColumn(
                name: "NewValue",
                table: "TranslationDictionarys",
                newName: "Value");
        }
    }
}
