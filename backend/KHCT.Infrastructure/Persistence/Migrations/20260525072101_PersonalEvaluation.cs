using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KHCT.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PersonalEvaluation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "personal_evaluation_period",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    department_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    report_year = table.Column<int>(type: "int", nullable: false),
                    report_month = table.Column<int>(type: "int", nullable: false),
                    capacity_attitude_self_score = table.Column<decimal>(type: "decimal(4,1)", nullable: true),
                    capacity_attitude_team_lead_score = table.Column<decimal>(type: "decimal(4,1)", nullable: true),
                    capacity_attitude_manager_score = table.Column<decimal>(type: "decimal(4,1)", nullable: true),
                    capacity_attitude_deputy_score = table.Column<decimal>(type: "decimal(4,1)", nullable: true),
                    capacity_attitude_head_score = table.Column<decimal>(type: "decimal(4,1)", nullable: true),
                    discipline_self_score = table.Column<decimal>(type: "decimal(4,1)", nullable: true),
                    discipline_team_lead_score = table.Column<decimal>(type: "decimal(4,1)", nullable: true),
                    discipline_manager_score = table.Column<decimal>(type: "decimal(4,1)", nullable: true),
                    discipline_deputy_score = table.Column<decimal>(type: "decimal(4,1)", nullable: true),
                    discipline_head_score = table.Column<decimal>(type: "decimal(4,1)", nullable: true),
                    inspection_self_score = table.Column<decimal>(type: "decimal(4,1)", nullable: true),
                    inspection_team_lead_score = table.Column<decimal>(type: "decimal(4,1)", nullable: true),
                    inspection_manager_score = table.Column<decimal>(type: "decimal(4,1)", nullable: true),
                    inspection_deputy_score = table.Column<decimal>(type: "decimal(4,1)", nullable: true),
                    inspection_head_score = table.Column<decimal>(type: "decimal(4,1)", nullable: true),
                    status = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_personal_evaluation_period", x => x.id);
                    table.ForeignKey(
                        name: "fk_personal_evaluation_period_app_user_user_id",
                        column: x => x.user_id,
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_personal_evaluation_period_department_department_id",
                        column: x => x.department_id,
                        principalTable: "department",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "personal_evaluation_item",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    period_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    display_order = table.Column<int>(type: "int", nullable: false),
                    assignment_source = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    task_name = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    task_detail = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    actual_result = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    note = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    deadline = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    completed_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    self_progress_score = table.Column<decimal>(type: "decimal(4,1)", nullable: true),
                    self_quality_score = table.Column<decimal>(type: "decimal(4,1)", nullable: true),
                    team_lead_progress_score = table.Column<decimal>(type: "decimal(4,1)", nullable: true),
                    team_lead_quality_score = table.Column<decimal>(type: "decimal(4,1)", nullable: true),
                    manager_progress_score = table.Column<decimal>(type: "decimal(4,1)", nullable: true),
                    manager_quality_score = table.Column<decimal>(type: "decimal(4,1)", nullable: true),
                    deputy_progress_score = table.Column<decimal>(type: "decimal(4,1)", nullable: true),
                    deputy_quality_score = table.Column<decimal>(type: "decimal(4,1)", nullable: true),
                    head_progress_score = table.Column<decimal>(type: "decimal(4,1)", nullable: true),
                    head_quality_score = table.Column<decimal>(type: "decimal(4,1)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_personal_evaluation_item", x => x.id);
                    table.ForeignKey(
                        name: "fk_personal_evaluation_item_personal_evaluation_period_period_id",
                        column: x => x.period_id,
                        principalTable: "personal_evaluation_period",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateIndex(
                name: "ix_personal_evaluation_item_period_id_display_order",
                table: "personal_evaluation_item",
                columns: new[] { "period_id", "display_order" });

            migrationBuilder.CreateIndex(
                name: "ix_personal_evaluation_period_department_id_report_year_report_~",
                table: "personal_evaluation_period",
                columns: new[] { "department_id", "report_year", "report_month" });

            migrationBuilder.CreateIndex(
                name: "ix_personal_evaluation_period_user_id_report_year_report_month",
                table: "personal_evaluation_period",
                columns: new[] { "user_id", "report_year", "report_month" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "personal_evaluation_item");

            migrationBuilder.DropTable(
                name: "personal_evaluation_period");
        }
    }
}
