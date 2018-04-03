using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace translate.web.Migrations
{
    public partial class MinorDocsChange : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsLoaded",
                table: "ProjectDocuments");

            migrationBuilder.RenameColumn(
                name: "FullPath",
                table: "ProjectDocuments",
                newName: "Header");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Header",
                table: "ProjectDocuments",
                newName: "FullPath");

            migrationBuilder.AddColumn<bool>(
                name: "IsLoaded",
                table: "ProjectDocuments",
                nullable: false,
                defaultValue: false);
        }
    }
}
