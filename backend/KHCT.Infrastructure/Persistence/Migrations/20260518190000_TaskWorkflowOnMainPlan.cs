using System;
using KHCT.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KHCT.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(KhctDbContext))]
    [Migration("20260518190000_TaskWorkflowOnMainPlan")]
    public partial class TaskWorkflowOnMainPlan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "approved_at",
                table: "task",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "approval_status",
                table: "task",
                type: "varchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Draft")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "submitted_at",
                table: "task",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "task_approval_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    task_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    department_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    action = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    from_status = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    to_status = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    actor_user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    comment = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_task_approval_history", x => x.id);
                    table.ForeignKey(
                        name: "fk_task_approval_history_department_department_id",
                        column: x => x.department_id,
                        principalTable: "department",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_task_approval_history_task_task_id",
                        column: x => x.task_id,
                        principalTable: "task",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_task_approval_history_app_user_actor_user_id",
                        column: x => x.actor_user_id,
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_task_approval_status",
                table: "task",
                column: "approval_status");

            migrationBuilder.CreateIndex(
                name: "ix_task_approval_history_actor_user_id",
                table: "task_approval_history",
                column: "actor_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_task_approval_history_department_id",
                table: "task_approval_history",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "ix_task_approval_history_task_id",
                table: "task_approval_history",
                column: "task_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "task_approval_history");

            migrationBuilder.DropIndex(
                name: "ix_task_approval_status",
                table: "task");

            migrationBuilder.DropColumn(
                name: "approved_at",
                table: "task");

            migrationBuilder.DropColumn(
                name: "approval_status",
                table: "task");

            migrationBuilder.DropColumn(
                name: "submitted_at",
                table: "task");
        }
    }
}
