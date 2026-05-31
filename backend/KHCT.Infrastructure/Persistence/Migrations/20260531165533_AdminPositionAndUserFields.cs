using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace KHCT.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdminPositionAndUserFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "last_logout_at",
                table: "app_user",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "position_id",
                table: "app_user",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "position",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    code = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    sort_order = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_position", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.UpdateData(
                table: "department",
                keyColumn: "id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000001"),
                column: "name",
                value: "Phòng KTNB1");

            migrationBuilder.UpdateData(
                table: "department",
                keyColumn: "id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000002"),
                column: "name",
                value: "Phòng KTNB2");

            migrationBuilder.UpdateData(
                table: "department",
                keyColumn: "id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000003"),
                column: "name",
                value: "Phòng KTNB3");

            migrationBuilder.UpdateData(
                table: "department",
                keyColumn: "id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000004"),
                column: "name",
                value: "Phòng KH");

            migrationBuilder.UpdateData(
                table: "department",
                keyColumn: "id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000006"),
                column: "name",
                value: "Phòng MN");

            migrationBuilder.UpdateData(
                table: "department",
                keyColumn: "id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000007"),
                column: "name",
                value: "Phòng MT");

            migrationBuilder.UpdateData(
                table: "department",
                keyColumn: "id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000008"),
                column: "name",
                value: "Phòng TNB");

            migrationBuilder.UpdateData(
                table: "department",
                keyColumn: "id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000009"),
                column: "name",
                value: "Phòng TKTH");

            migrationBuilder.InsertData(
                table: "department",
                columns: new[] { "id", "code", "created_at", "is_active", "name", "updated_at" },
                values: new object[,]
                {
                    { new Guid("11111111-0000-0000-0000-000000000010"), "LDKTNB", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Lãnh đạo KTNB", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-0000-0000-0000-000000000011"), "BKS", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Ban KS", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-0000-0000-0000-000000000012"), "VL", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Vãng lai", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "position",
                columns: new[] { "id", "code", "created_at", "is_active", "name", "sort_order", "updated_at" },
                values: new object[,]
                {
                    { new Guid("55555555-0000-0000-0000-000000000001"), "TRUONG_KTNB", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Trưởng KTNB", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("55555555-0000-0000-0000-000000000002"), "PHO_TRUONG_KTNB", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Phó Trưởng KTNB", 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("55555555-0000-0000-0000-000000000003"), "TRUONG_BKS", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Trưởng BKS", 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("55555555-0000-0000-0000-000000000004"), "THANH_VIEN_BKS", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Thành viên BKS", 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("55555555-0000-0000-0000-000000000005"), "TRUONG_PHONG", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Trưởng Phòng", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("55555555-0000-0000-0000-000000000006"), "PHO_PHONG", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Phó Phòng", 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("55555555-0000-0000-0000-000000000007"), "TRUONG_NHOM", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Trưởng nhóm", 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("55555555-0000-0000-0000-000000000008"), "NHAN_VIEN", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Nhân viên", 8, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.UpdateData(
                table: "role",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000004"),
                column: "name",
                value: "Phê duyệt 1 – Trưởng KTNB");

            migrationBuilder.UpdateData(
                table: "role",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000005"),
                column: "name",
                value: "Phê duyệt 2 – Phó Trưởng KTNB");

            migrationBuilder.UpdateData(
                table: "role",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000006"),
                column: "name",
                value: "Kiểm soát 1 – Trưởng phòng");

            migrationBuilder.UpdateData(
                table: "role",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000007"),
                column: "name",
                value: "Kiểm soát 2 – Phó phòng / Trưởng nhóm");

            migrationBuilder.UpdateData(
                table: "role",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000009"),
                column: "name",
                value: "Giám sát 2 – Guest");

            migrationBuilder.CreateIndex(
                name: "ix_app_user_position_id",
                table: "app_user",
                column: "position_id");

            migrationBuilder.CreateIndex(
                name: "ix_position_code",
                table: "position",
                column: "code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_app_user_position_position_id",
                table: "app_user",
                column: "position_id",
                principalTable: "position",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_app_user_position_position_id",
                table: "app_user");

            migrationBuilder.DropTable(
                name: "position");

            migrationBuilder.DropIndex(
                name: "ix_app_user_position_id",
                table: "app_user");

            migrationBuilder.DeleteData(
                table: "department",
                keyColumn: "id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000010"));

            migrationBuilder.DeleteData(
                table: "department",
                keyColumn: "id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000011"));

            migrationBuilder.DeleteData(
                table: "department",
                keyColumn: "id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000012"));

            migrationBuilder.DropColumn(
                name: "last_logout_at",
                table: "app_user");

            migrationBuilder.DropColumn(
                name: "position_id",
                table: "app_user");

            migrationBuilder.UpdateData(
                table: "department",
                keyColumn: "id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000001"),
                column: "name",
                value: "Kiểm toán nội bộ 1");

            migrationBuilder.UpdateData(
                table: "department",
                keyColumn: "id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000002"),
                column: "name",
                value: "Kiểm toán nội bộ 2");

            migrationBuilder.UpdateData(
                table: "department",
                keyColumn: "id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000003"),
                column: "name",
                value: "Kiểm toán nội bộ 3");

            migrationBuilder.UpdateData(
                table: "department",
                keyColumn: "id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000004"),
                column: "name",
                value: "Phòng Kế hoạch");

            migrationBuilder.UpdateData(
                table: "department",
                keyColumn: "id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000006"),
                column: "name",
                value: "Văn phòng miền Nam");

            migrationBuilder.UpdateData(
                table: "department",
                keyColumn: "id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000007"),
                column: "name",
                value: "Văn phòng miền Trung");

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

            migrationBuilder.UpdateData(
                table: "role",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000004"),
                column: "name",
                value: "Trưởng KTNB");

            migrationBuilder.UpdateData(
                table: "role",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000005"),
                column: "name",
                value: "Phó Trưởng KTNB");

            migrationBuilder.UpdateData(
                table: "role",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000006"),
                column: "name",
                value: "Trưởng phòng");

            migrationBuilder.UpdateData(
                table: "role",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000007"),
                column: "name",
                value: "Phó phòng / Trưởng nhóm");

            migrationBuilder.UpdateData(
                table: "role",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000009"),
                column: "name",
                value: "Người giám sát");
        }
    }
}
