using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QTKD.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddContractSubmissionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "Contracts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DocumentName",
                table: "Contracts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Contracts",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Content", "DocumentName" },
                values: new object[] { "", "" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "DocumentName",
                table: "Contracts");
        }
    }
}
