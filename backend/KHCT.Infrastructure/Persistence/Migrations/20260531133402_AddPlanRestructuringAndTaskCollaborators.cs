using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KHCT.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPlanRestructuringAndTaskCollaborators : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "complexity",
                table: "task",
                type: "varchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "",
                collation: "utf8mb4_0900_ai_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "priority",
                table: "task",
                type: "varchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "",
                collation: "utf8mb4_0900_ai_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "current_period_index",
                table: "plan",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "ktnb_leader_id",
                table: "plan",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "plan",
                type: "longtext",
                nullable: false,
                collation: "utf8mb4_0900_ai_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "reporting_period_type",
                table: "plan",
                type: "varchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "",
                collation: "utf8mb4_0900_ai_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "plan_reporting_period",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    plan_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    period_label = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    progress_text = table.Column<string>(type: "varchar(2048)", maxLength: 2048, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    completion_percent = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    approved_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    approved_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_plan_reporting_period", x => x.id);
                    table.ForeignKey(
                        name: "fk_plan_reporting_period_app_user_approved_by_user_id",
                        column: x => x.approved_by_user_id,
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_plan_reporting_period_plan_plan_id",
                        column: x => x.plan_id,
                        principalTable: "plan",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "task_collaborator",
                columns: table => new
                {
                    task_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    role = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    collaboration_content = table.Column<string>(type: "varchar(2048)", maxLength: 2048, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_task_collaborator", x => new { x.task_id, x.user_id });
                    table.ForeignKey(
                        name: "fk_task_collaborator_app_user_user_id",
                        column: x => x.user_id,
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_task_collaborator_task_task_id",
                        column: x => x.task_id,
                        principalTable: "task",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateIndex(
                name: "ix_plan_ktnb_leader_id",
                table: "plan",
                column: "ktnb_leader_id");

            migrationBuilder.CreateIndex(
                name: "ix_plan_reporting_period_approved_by_user_id",
                table: "plan_reporting_period",
                column: "approved_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_plan_reporting_period_plan_id",
                table: "plan_reporting_period",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "ix_task_collaborator_user_id",
                table: "task_collaborator",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "fk_plan_app_user_ktnb_leader_id",
                table: "plan",
                column: "ktnb_leader_id",
                principalTable: "app_user",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_plan_app_user_ktnb_leader_id",
                table: "plan");

            migrationBuilder.DropTable(
                name: "plan_reporting_period");

            migrationBuilder.DropTable(
                name: "task_collaborator");

            migrationBuilder.DropIndex(
                name: "ix_plan_ktnb_leader_id",
                table: "plan");

            migrationBuilder.DropColumn(
                name: "complexity",
                table: "task");

            migrationBuilder.DropColumn(
                name: "priority",
                table: "task");

            migrationBuilder.DropColumn(
                name: "current_period_index",
                table: "plan");

            migrationBuilder.DropColumn(
                name: "ktnb_leader_id",
                table: "plan");

            migrationBuilder.DropColumn(
                name: "name",
                table: "plan");

            migrationBuilder.DropColumn(
                name: "reporting_period_type",
                table: "plan");
        }
    }
}
