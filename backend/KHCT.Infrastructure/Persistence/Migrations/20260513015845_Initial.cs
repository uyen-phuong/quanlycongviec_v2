using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace KHCT.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "bks_member",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    full_name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    title = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bks_member", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "department",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    code = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_department", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "role",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    code = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "app_user",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    username = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    password_hash = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    full_name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    department_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    last_login_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_user", x => x.id);
                    table.ForeignKey(
                        name: "fk_app_user_department_department_id",
                        column: x => x.department_id,
                        principalTable: "department",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "approval_config",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    scope = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    level = table.Column<int>(type: "int", nullable: false),
                    department_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    role_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_approval_config", x => x.id);
                    table.ForeignKey(
                        name: "fk_approval_config_department_department_id",
                        column: x => x.department_id,
                        principalTable: "department",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_approval_config_role_role_id",
                        column: x => x.role_id,
                        principalTable: "role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "attachment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    owner_type = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    owner_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    file_name = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    stored_path = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    content_type = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    uploaded_by_user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_attachment", x => x.id);
                    table.ForeignKey(
                        name: "fk_attachment_app_user_uploaded_by_user_id",
                        column: x => x.uploaded_by_user_id,
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "audit_log",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    entity_name = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    entity_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    action = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    actor_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    before_json = table.Column<string>(type: "json", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    after_json = table.Column<string>(type: "json", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_log", x => x.id);
                    table.ForeignKey(
                        name: "fk_audit_log_app_user_actor_user_id",
                        column: x => x.actor_user_id,
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "plan",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    scope = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    department_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    year = table.Column<int>(type: "int", nullable: false),
                    month = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_by_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    submitted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    approved_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_plan", x => x.id);
                    table.ForeignKey(
                        name: "fk_plan_app_user_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_plan_department_department_id",
                        column: x => x.department_id,
                        principalTable: "department",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "refresh_token",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    token_hash = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    expires_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    revoked_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    replaced_by_token_hash = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_by_ip = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_refresh_token", x => x.id);
                    table.ForeignKey(
                        name: "fk_refresh_token_app_user_user_id",
                        column: x => x.user_id,
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "user_role",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    role_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_role", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "fk_user_role_app_user_user_id",
                        column: x => x.user_id,
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_role_role_role_id",
                        column: x => x.role_id,
                        principalTable: "role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "approval_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    plan_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    action = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    from_status = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    to_status = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    actor_user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    comment = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_approval_history", x => x.id);
                    table.ForeignKey(
                        name: "fk_approval_history_app_user_actor_user_id",
                        column: x => x.actor_user_id,
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_approval_history_plan_plan_id",
                        column: x => x.plan_id,
                        principalTable: "plan",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "task",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    plan_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    parent_task_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    outline_index = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    display_order = table.Column<int>(type: "int", nullable: false),
                    is_header = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    title = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    work_type = table.Column<int>(type: "int", nullable: false),
                    work_status = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    deadline = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    assignee_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    bks_member_text = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_locked = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    inherited_from_task_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    has_open_comment = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    reason_not_completed = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    progress_text = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_task", x => x.id);
                    table.ForeignKey(
                        name: "fk_task_app_user_assignee_user_id",
                        column: x => x.assignee_user_id,
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_task_plan_plan_id",
                        column: x => x.plan_id,
                        principalTable: "plan",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_task_task_inherited_from_task_id",
                        column: x => x.inherited_from_task_id,
                        principalTable: "task",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_task_task_parent_task_id",
                        column: x => x.parent_task_id,
                        principalTable: "task",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "line_comment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    task_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    parent_comment_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    author_user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    author_role = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    content = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_resolved = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    resolved_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    resolved_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_line_comment", x => x.id);
                    table.ForeignKey(
                        name: "fk_line_comment_app_user_author_user_id",
                        column: x => x.author_user_id,
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_line_comment_app_user_resolved_by_user_id",
                        column: x => x.resolved_by_user_id,
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_line_comment_line_comment_parent_comment_id",
                        column: x => x.parent_comment_id,
                        principalTable: "line_comment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_line_comment_task_task_id",
                        column: x => x.task_id,
                        principalTable: "task",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "task_supporting_dept",
                columns: table => new
                {
                    task_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    department_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_task_supporting_dept", x => new { x.task_id, x.department_id });
                    table.ForeignKey(
                        name: "fk_task_supporting_dept_department_department_id",
                        column: x => x.department_id,
                        principalTable: "department",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_task_supporting_dept_task_task_id",
                        column: x => x.task_id,
                        principalTable: "task",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.InsertData(
                table: "department",
                columns: new[] { "id", "code", "created_at", "is_active", "name", "updated_at" },
                values: new object[,]
                {
                    { new Guid("11111111-0000-0000-0000-000000000001"), "KTNB1", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Kiểm toán nội bộ 1", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-0000-0000-0000-000000000002"), "KTNB2", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Kiểm toán nội bộ 2", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-0000-0000-0000-000000000003"), "KTNB3", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Kiểm toán nội bộ 3", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-0000-0000-0000-000000000004"), "KH", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Phòng Kế hoạch", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-0000-0000-0000-000000000005"), "GS", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Phòng Giám sát", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-0000-0000-0000-000000000006"), "VPMN", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Văn phòng miền Nam", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-0000-0000-0000-000000000007"), "VPMT", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Văn phòng miền Trung", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-0000-0000-0000-000000000008"), "VPTNB", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Văn phòng Tây Nam Bộ", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-0000-0000-0000-000000000009"), "TKTH", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Bộ phận Thư ký tổng hợp", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "role",
                columns: new[] { "id", "code", "created_at", "name", "updated_at" },
                values: new object[,]
                {
                    { new Guid("22222222-0000-0000-0000-000000000001"), "ADMIN", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Quản trị hệ thống", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("22222222-0000-0000-0000-000000000002"), "VAN_THU", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Văn thư", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("22222222-0000-0000-0000-000000000003"), "TRUONG_KH", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Trưởng phòng Kế hoạch", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("22222222-0000-0000-0000-000000000004"), "TRUONG_KTNB", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Trưởng KTNB", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("22222222-0000-0000-0000-000000000005"), "PHO_TRUONG_KTNB", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Phó Trưởng KTNB", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("22222222-0000-0000-0000-000000000006"), "TRUONG_PHONG", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Trưởng phòng", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("22222222-0000-0000-0000-000000000007"), "TRUONG_NHOM", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Trưởng nhóm", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("22222222-0000-0000-0000-000000000008"), "NHAN_VIEN", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Nhân viên", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "approval_config",
                columns: new[] { "id", "created_at", "department_id", "is_active", "level", "role_id", "scope", "updated_at" },
                values: new object[,]
                {
                    { new Guid("44444444-0000-0000-0000-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, 1, new Guid("22222222-0000-0000-0000-000000000003"), "Main", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("44444444-0000-0000-0000-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, 2, new Guid("22222222-0000-0000-0000-000000000004"), "Main", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("44444444-0000-0000-0000-000000000003"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, 1, new Guid("22222222-0000-0000-0000-000000000007"), "Sub", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("44444444-0000-0000-0000-000000000004"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, 2, new Guid("22222222-0000-0000-0000-000000000006"), "Sub", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("44444444-0000-0000-0000-000000000005"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, 3, new Guid("22222222-0000-0000-0000-000000000005"), "Sub", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "ix_app_user_department_id",
                table: "app_user",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "ix_app_user_username",
                table: "app_user",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_approval_config_department_id",
                table: "approval_config",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "ix_approval_config_role_id",
                table: "approval_config",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_approval_config_scope_level_department_id_role_id",
                table: "approval_config",
                columns: new[] { "scope", "level", "department_id", "role_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_approval_history_actor_user_id",
                table: "approval_history",
                column: "actor_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_approval_history_plan_id",
                table: "approval_history",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "ix_attachment_owner_type_owner_id",
                table: "attachment",
                columns: new[] { "owner_type", "owner_id" });

            migrationBuilder.CreateIndex(
                name: "ix_attachment_uploaded_by_user_id",
                table: "attachment",
                column: "uploaded_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_audit_log_actor_user_id",
                table: "audit_log",
                column: "actor_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_audit_log_entity_name_entity_id",
                table: "audit_log",
                columns: new[] { "entity_name", "entity_id" });

            migrationBuilder.CreateIndex(
                name: "ix_department_code",
                table: "department",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_line_comment_author_user_id",
                table: "line_comment",
                column: "author_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_line_comment_parent_comment_id",
                table: "line_comment",
                column: "parent_comment_id");

            migrationBuilder.CreateIndex(
                name: "ix_line_comment_resolved_by_user_id",
                table: "line_comment",
                column: "resolved_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_line_comment_task_id",
                table: "line_comment",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "ix_plan_created_by_id",
                table: "plan",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_plan_department_id",
                table: "plan",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "ix_plan_scope_department_id_year_month",
                table: "plan",
                columns: new[] { "scope", "department_id", "year", "month" });

            migrationBuilder.CreateIndex(
                name: "ix_refresh_token_token_hash",
                table: "refresh_token",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_refresh_token_user_id",
                table: "refresh_token",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_role_code",
                table: "role",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_task_assignee_user_id",
                table: "task",
                column: "assignee_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_task_inherited_from_task_id",
                table: "task",
                column: "inherited_from_task_id");

            migrationBuilder.CreateIndex(
                name: "ix_task_parent_task_id",
                table: "task",
                column: "parent_task_id");

            migrationBuilder.CreateIndex(
                name: "ix_task_plan_id",
                table: "task",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "ix_task_supporting_dept_department_id",
                table: "task_supporting_dept",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_role_role_id",
                table: "user_role",
                column: "role_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "approval_config");

            migrationBuilder.DropTable(
                name: "approval_history");

            migrationBuilder.DropTable(
                name: "attachment");

            migrationBuilder.DropTable(
                name: "audit_log");

            migrationBuilder.DropTable(
                name: "bks_member");

            migrationBuilder.DropTable(
                name: "line_comment");

            migrationBuilder.DropTable(
                name: "refresh_token");

            migrationBuilder.DropTable(
                name: "task_supporting_dept");

            migrationBuilder.DropTable(
                name: "user_role");

            migrationBuilder.DropTable(
                name: "task");

            migrationBuilder.DropTable(
                name: "role");

            migrationBuilder.DropTable(
                name: "plan");

            migrationBuilder.DropTable(
                name: "app_user");

            migrationBuilder.DropTable(
                name: "department");
        }
    }
}
