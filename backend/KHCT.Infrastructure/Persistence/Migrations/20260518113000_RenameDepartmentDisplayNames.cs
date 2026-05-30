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
            migrationBuilder.Sql("UPDATE department SET name = 'Văn phòng Tây Nam Bộ' WHERE id = '11111111-0000-0000-0000-000000000008';");
            migrationBuilder.Sql("UPDATE department SET name = 'Bộ phận Thư ký tổng hợp' WHERE id = '11111111-0000-0000-0000-000000000009';");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE department SET name = 'Văn phòng Trưởng KTNB' WHERE id = '11111111-0000-0000-0000-000000000008';");
            migrationBuilder.Sql("UPDATE department SET name = 'Tổng hợp' WHERE id = '11111111-0000-0000-0000-000000000009';");
        }
    }
}
