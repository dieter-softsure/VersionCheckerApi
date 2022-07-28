using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VersionCheckerApi.Migrations
{
    public partial class doublekey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Packages_Modules_ModuleId",
                table: "Packages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Packages",
                table: "Packages");

            migrationBuilder.DropIndex(
                name: "IX_Packages_ModuleId",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "PackageId",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "ModuleId",
                table: "Packages");

            migrationBuilder.AlterColumn<string>(
                name: "Version",
                table: "Packages",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)")
                .Annotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Packages",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)")
                .Annotation("Relational:ColumnOrder", 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Packages",
                table: "Packages",
                columns: new[] { "Name", "Version" });

            migrationBuilder.CreateTable(
                name: "ModulePackage",
                columns: table => new
                {
                    ModulesModuleId = table.Column<int>(type: "int", nullable: false),
                    PackagesName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PackagesVersion = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModulePackage", x => new { x.ModulesModuleId, x.PackagesName, x.PackagesVersion });
                    table.ForeignKey(
                        name: "FK_ModulePackage_Modules_ModulesModuleId",
                        column: x => x.ModulesModuleId,
                        principalTable: "Modules",
                        principalColumn: "ModuleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ModulePackage_Packages_PackagesName_PackagesVersion",
                        columns: x => new { x.PackagesName, x.PackagesVersion },
                        principalTable: "Packages",
                        principalColumns: new[] { "Name", "Version" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ModulePackage_PackagesName_PackagesVersion",
                table: "ModulePackage",
                columns: new[] { "PackagesName", "PackagesVersion" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModulePackage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Packages",
                table: "Packages");

            migrationBuilder.AlterColumn<string>(
                name: "Version",
                table: "Packages",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)")
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Packages",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)")
                .OldAnnotation("Relational:ColumnOrder", 0);

            migrationBuilder.AddColumn<string>(
                name: "PackageId",
                table: "Packages",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ModuleId",
                table: "Packages",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Packages",
                table: "Packages",
                column: "PackageId");

            migrationBuilder.CreateIndex(
                name: "IX_Packages_ModuleId",
                table: "Packages",
                column: "ModuleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Packages_Modules_ModuleId",
                table: "Packages",
                column: "ModuleId",
                principalTable: "Modules",
                principalColumn: "ModuleId");
        }
    }
}
