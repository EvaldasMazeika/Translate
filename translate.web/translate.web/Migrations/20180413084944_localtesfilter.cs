using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace translate.web.Migrations
{
    public partial class localtesfilter : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ShowOnlyMine",
                table: "ProjectMembers",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShowOnlyMine",
                table: "ProjectMembers");
        }
    }
}
