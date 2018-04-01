using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace translate.web.Migrations
{
    public partial class translationEdit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeclineComment",
                table: "Translations",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsWaiting",
                table: "Translations",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeclineComment",
                table: "Translations");

            migrationBuilder.DropColumn(
                name: "IsWaiting",
                table: "Translations");
        }
    }
}
