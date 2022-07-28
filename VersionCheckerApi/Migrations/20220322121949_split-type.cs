using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VersionCheckerApi.Migrations
{
    public partial class splittype : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ModulePackage_Packages_PackagesName_PackagesVersion",
                table: "ModulePackage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Packages",
                table: "Packages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ModulePackage",
                table: "ModulePackage");

            migrationBuilder.DropIndex(
                name: "IX_ModulePackage_PackagesName_PackagesVersion",
                table: "ModulePackage");

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "Packages",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<string>(
                name: "Tags",
                table: "Packages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullPath",
                table: "Modules",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PackagesType",
                table: "ModulePackage",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Packages",
                table: "Packages",
                columns: new[] { "Name", "Version", "Type" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ModulePackage",
                table: "ModulePackage",
                columns: new[] { "ModulesModuleId", "PackagesName", "PackagesVersion", "PackagesType" });

            migrationBuilder.CreateIndex(
                name: "IX_ModulePackage_PackagesName_PackagesVersion_PackagesType",
                table: "ModulePackage",
                columns: new[] { "PackagesName", "PackagesVersion", "PackagesType" });

            migrationBuilder.AddForeignKey(
                name: "FK_ModulePackage_Packages_PackagesName_PackagesVersion_PackagesType",
                table: "ModulePackage",
                columns: new[] { "PackagesName", "PackagesVersion", "PackagesType" },
                principalTable: "Packages",
                principalColumns: new[] { "Name", "Version", "Type" },
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ModulePackage_Packages_PackagesName_PackagesVersion_PackagesType",
                table: "ModulePackage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Packages",
                table: "Packages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ModulePackage",
                table: "ModulePackage");

            migrationBuilder.DropIndex(
                name: "IX_ModulePackage_PackagesName_PackagesVersion_PackagesType",
                table: "ModulePackage");

            migrationBuilder.DropColumn(
                name: "FullPath",
                table: "Modules");

            migrationBuilder.DropColumn(
                name: "PackagesType",
                table: "ModulePackage");

            migrationBuilder.AlterColumn<string>(
                name: "Tags",
                table: "Packages",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "Packages",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Packages",
                table: "Packages",
                columns: new[] { "Name", "Version" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ModulePackage",
                table: "ModulePackage",
                columns: new[] { "ModulesModuleId", "PackagesName", "PackagesVersion" });

            migrationBuilder.CreateIndex(
                name: "IX_ModulePackage_PackagesName_PackagesVersion",
                table: "ModulePackage",
                columns: new[] { "PackagesName", "PackagesVersion" });

            migrationBuilder.AddForeignKey(
                name: "FK_ModulePackage_Packages_PackagesName_PackagesVersion",
                table: "ModulePackage",
                columns: new[] { "PackagesName", "PackagesVersion" },
                principalTable: "Packages",
                principalColumns: new[] { "Name", "Version" },
                onDelete: ReferentialAction.Cascade);
        }
    }
}
