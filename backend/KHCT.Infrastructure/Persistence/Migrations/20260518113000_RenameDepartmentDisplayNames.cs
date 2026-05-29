using System;
using KHCT.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KHCT.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(KhctDbContext))]
    [Migration("20260518113000_RenameDepartmentDisplayNames")]
    public partial class RenameDepartmentDisplayNames : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "department",
                keyColumn: "id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000008"),
                column: "name",
                value: "Văn phòng Tây Nam Bộ");

            migrationBuilder.UpdateData(
                table: "department",
                keyColumn: "id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000009"),
                column: "name",
                value: "Bộ phận Thư ký tổng hợp");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "department",
                keyColumn: "id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000008"),
                column: "name",
                value: "Văn phòng Trưởng KTNB");

            migrationBuilder.UpdateData(
                table: "department",
                keyColumn: "id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000009"),
                column: "name",
                value: "Tổng hợp");
        }
    }
}
