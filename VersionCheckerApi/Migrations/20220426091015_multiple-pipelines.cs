using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VersionCheckerApi.Migrations
{
    public partial class multiplepipelines : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Pipelines_projectId",
                table: "Pipelines");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Pipelines",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Pipelines_projectId",
                table: "Pipelines",
                column: "projectId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Pipelines_projectId",
                table: "Pipelines");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Pipelines");

            migrationBuilder.CreateIndex(
                name: "IX_Pipelines_projectId",
                table: "Pipelines",
                column: "projectId",
                unique: true);
        }
    }
}
