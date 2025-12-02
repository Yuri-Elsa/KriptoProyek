using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KriptoProyek.Migrations
{
    /// <inheritdoc />
    public partial class FixCreatedAtStatic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1",
                column: "ConcurrencyStamp",
                value: "966a8a2a-3b9d-430e-b58e-e2a90036d936");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2",
                column: "ConcurrencyStamp",
                value: "864e3672-0ee9-4d7e-a3bc-b69f0ef0ee24");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1",
                column: "ConcurrencyStamp",
                value: "9e3696e1-d309-4280-bc9f-ac24a468157e");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2",
                column: "ConcurrencyStamp",
                value: "03407900-9f33-4857-ac64-ae4c4cfd1775");
        }
    }
}
