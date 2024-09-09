using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PackAndGo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPackingLists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("2fc56300-17dd-4a48-9793-363f14f29a30"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("51181cb1-4e76-4402-aee6-aac3405ca654"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("8a468c35-db94-4e17-ae84-a7b25f581e8f"));

            migrationBuilder.CreateTable(
                name: "PackingLists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackingLists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    IsPacked = table.Column<bool>(type: "INTEGER", nullable: false),
                    PackingListDataModelId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Items_PackingLists_PackingListDataModelId",
                        column: x => x.PackingListDataModelId,
                        principalTable: "PackingLists",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email" },
                values: new object[,]
                {
                    { new Guid("097498c7-66ad-4a1f-b1a4-0843e89a78e2"), "john.doe@test.com" },
                    { new Guid("0c0e1af3-d9aa-4b86-a41b-fb44cdb685a4"), "jane.doe@test.com" },
                    { new Guid("5458a1ce-d74f-42d8-b83d-b917cc21a8f9"), "jacob.doe@test.com" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Items_PackingListDataModelId",
                table: "Items",
                column: "PackingListDataModelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "PackingLists");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("097498c7-66ad-4a1f-b1a4-0843e89a78e2"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("0c0e1af3-d9aa-4b86-a41b-fb44cdb685a4"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("5458a1ce-d74f-42d8-b83d-b917cc21a8f9"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email" },
                values: new object[,]
                {
                    { new Guid("2fc56300-17dd-4a48-9793-363f14f29a30"), "jane.doe@test.com" },
                    { new Guid("51181cb1-4e76-4402-aee6-aac3405ca654"), "john.doe@test.com" },
                    { new Guid("8a468c35-db94-4e17-ae84-a7b25f581e8f"), "jacob.doe@test.com" }
                });
        }
    }
}
