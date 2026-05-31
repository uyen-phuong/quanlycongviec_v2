using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KHCT.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTaskWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "approval_status",
                table: "task",
                newName: "workflow_status");

            migrationBuilder.RenameIndex(
                name: "ix_task_approval_status",
                table: "task",
                newName: "ix_task_workflow_status");

            migrationBuilder.AlterColumn<Guid>(
                name: "plan_id",
                table: "task",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AddColumn<string>(
                name: "category",
                table: "task",
                type: "varchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "",
                collation: "utf8mb4_0900_ai_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "controller_user_id",
                table: "task",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "project_id",
                table: "task",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.UpdateData(
                table: "approval_config",
                keyColumn: "id",
                keyValue: new Guid("44444444-0000-0000-0000-000000000001"),
                columns: new[] { "department_id", "role_id" },
                values: new object[] { new Guid("11111111-0000-0000-0000-000000000004"), new Guid("22222222-0000-0000-0000-000000000006") });

            migrationBuilder.DeleteData(
                table: "role",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000003"));

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
                keyValue: new Guid("11111111-0000-0000-0000-000000000005"),
                column: "name",
                value: "Phòng Giám sát");

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
                keyValue: new Guid("22222222-0000-0000-0000-000000000001"),
                column: "name",
                value: "Quản trị hệ thống");

            migrationBuilder.UpdateData(
                table: "role",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000002"),
                column: "name",
                value: "Văn thư");

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
                columns: new[] { "code", "name" },
                values: new object[] { "PHO_PHONG", "Phó phòng / Trưởng nhóm" });

            migrationBuilder.UpdateData(
                table: "role",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000008"),
                column: "name",
                value: "Nhân viên");

            migrationBuilder.InsertData(
                table: "role",
                columns: new[] { "id", "code", "created_at", "name", "updated_at" },
                values: new object[] { new Guid("22222222-0000-0000-0000-000000000009"), "GUEST", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Người giám sát", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.CreateIndex(
                name: "ix_task_controller_user_id",
                table: "task",
                column: "controller_user_id");

            migrationBuilder.AddForeignKey(
                name: "fk_task_app_user_controller_user_id",
                table: "task",
                column: "controller_user_id",
                principalTable: "app_user",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_task_app_user_controller_user_id",
                table: "task");

            migrationBuilder.DropIndex(
                name: "ix_task_controller_user_id",
                table: "task");

            migrationBuilder.DeleteData(
                table: "role",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000009"));

            migrationBuilder.DropColumn(
                name: "category",
                table: "task");

            migrationBuilder.DropColumn(
                name: "controller_user_id",
                table: "task");

            migrationBuilder.DropColumn(
                name: "project_id",
                table: "task");

            migrationBuilder.RenameColumn(
                name: "workflow_status",
                table: "task",
                newName: "approval_status");

            migrationBuilder.RenameIndex(
                name: "ix_task_workflow_status",
                table: "task",
                newName: "ix_task_approval_status");

            migrationBuilder.AlterColumn<Guid>(
                name: "plan_id",
                table: "task",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.UpdateData(
                table: "approval_config",
                keyColumn: "id",
                keyValue: new Guid("44444444-0000-0000-0000-000000000001"),
                columns: new[] { "department_id", "role_id" },
                values: new object[] { null, new Guid("22222222-0000-0000-0000-000000000003") });

            migrationBuilder.UpdateData(
                table: "department",
                keyColumn: "id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000001"),
                column: "name",
                value: "Ki?m to�n n?i b? 1");

            migrationBuilder.UpdateData(
                table: "department",
                keyColumn: "id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000002"),
                column: "name",
                value: "Ki?m to�n n?i b? 2");

            migrationBuilder.UpdateData(
                table: "department",
                keyColumn: "id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000003"),
                column: "name",
                value: "Ki?m to�n n?i b? 3");

            migrationBuilder.UpdateData(
                table: "department",
                keyColumn: "id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000004"),
                column: "name",
                value: "Ph�ng K? ho?ch");

            migrationBuilder.UpdateData(
                table: "department",
                keyColumn: "id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000005"),
                column: "name",
                value: "Ph�ng Gi�m s�t");

            migrationBuilder.UpdateData(
                table: "department",
                keyColumn: "id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000006"),
                column: "name",
                value: "Van ph�ng mi?n Nam");

            migrationBuilder.UpdateData(
                table: "department",
                keyColumn: "id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000007"),
                column: "name",
                value: "Van ph�ng mi?n Trung");

            migrationBuilder.UpdateData(
                table: "department",
                keyColumn: "id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000008"),
                column: "name",
                value: "Van ph�ng T�y Nam B?");

            migrationBuilder.UpdateData(
                table: "department",
                keyColumn: "id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000009"),
                column: "name",
                value: "B? ph?n Thu k� t?ng h?p");

            migrationBuilder.UpdateData(
                table: "role",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000001"),
                column: "name",
                value: "Qu?n tr? h? th?ng");

            migrationBuilder.UpdateData(
                table: "role",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000002"),
                column: "name",
                value: "Van thu");

            migrationBuilder.UpdateData(
                table: "role",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000004"),
                column: "name",
                value: "Tru?ng KTNB");

            migrationBuilder.UpdateData(
                table: "role",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000005"),
                column: "name",
                value: "Ph� Tru?ng KTNB");

            migrationBuilder.UpdateData(
                table: "role",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000006"),
                column: "name",
                value: "Tru?ng ph�ng");

            migrationBuilder.UpdateData(
                table: "role",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000007"),
                columns: new[] { "code", "name" },
                values: new object[] { "TRUONG_NHOM", "Tru?ng nh�m" });

            migrationBuilder.UpdateData(
                table: "role",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000008"),
                column: "name",
                value: "Nh�n vi�n");

            migrationBuilder.InsertData(
                table: "role",
                columns: new[] { "id", "code", "created_at", "name", "updated_at" },
                values: new object[] { new Guid("22222222-0000-0000-0000-000000000003"), "TRUONG_KH", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tru?ng ph�ng K? ho?ch", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });
        }
    }
}
