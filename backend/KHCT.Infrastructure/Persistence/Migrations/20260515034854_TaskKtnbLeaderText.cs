using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KHCT.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TaskKtnbLeaderText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ktnb_leader_text",
                table: "task",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true,
                collation: "utf8mb4_0900_ai_ci")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ktnb_leader_text",
                table: "task");
        }
    }
}
