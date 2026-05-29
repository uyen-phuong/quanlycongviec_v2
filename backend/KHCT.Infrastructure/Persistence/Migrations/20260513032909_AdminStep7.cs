using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KHCT.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdminStep7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_approval_config_scope_level_department_id_role_id",
                table: "approval_config");

            migrationBuilder.CreateIndex(
                name: "ix_approval_config_scope_level",
                table: "approval_config",
                columns: new[] { "scope", "level" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_approval_config_scope_level",
                table: "approval_config");

            migrationBuilder.CreateIndex(
                name: "ix_approval_config_scope_level_department_id_role_id",
                table: "approval_config",
                columns: new[] { "scope", "level", "department_id", "role_id" },
                unique: true);
        }
    }
}
