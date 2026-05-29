using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KHCT.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TaskOwnerDepartmentAndInheritedTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "owner_department_id",
                table: "task",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "ix_task_owner_department_id",
                table: "task",
                column: "owner_department_id");

            migrationBuilder.AddForeignKey(
                name: "fk_task_department_owner_department_id",
                table: "task",
                column: "owner_department_id",
                principalTable: "department",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_task_department_owner_department_id",
                table: "task");

            migrationBuilder.DropIndex(
                name: "ix_task_owner_department_id",
                table: "task");

            migrationBuilder.DropColumn(
                name: "owner_department_id",
                table: "task");
        }
    }
}
