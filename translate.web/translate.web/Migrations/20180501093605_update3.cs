using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace translate.web.Migrations
{
    public partial class update3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Translations");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Translations",
                nullable: true);
        }
    }
}
