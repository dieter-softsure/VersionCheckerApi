using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VersionCheckerApi.Migrations
{
    public partial class removemoduleid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Packages_Modules_ModuleId",
                table: "Packages");

            migrationBuilder.AlterColumn<int>(
                name: "ModuleId",
                table: "Packages",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Packages_Modules_ModuleId",
                table: "Packages",
                column: "ModuleId",
                principalTable: "Modules",
                principalColumn: "ModuleId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Packages_Modules_ModuleId",
                table: "Packages");

            migrationBuilder.AlterColumn<int>(
                name: "ModuleId",
                table: "Packages",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Packages_Modules_ModuleId",
                table: "Packages",
                column: "ModuleId",
                principalTable: "Modules",
                principalColumn: "ModuleId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
