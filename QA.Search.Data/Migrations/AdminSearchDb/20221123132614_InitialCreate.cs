using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace QA.Search.Data.Migrations.AdminSearchDb
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "admin");

            migrationBuilder.CreateTable(
                name: "reindex_tasks",
                schema: "admin",
                columns: table => new
                {
                    source_index = table.Column<string>(type: "text", nullable: false),
                    destination_index = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    short_index_name = table.Column<string>(type: "text", nullable: true),
                    elastic_task_id = table.Column<string>(type: "text", nullable: true),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    finished = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    timestamp = table.Column<long>(type: "bigint", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reindex_tasks", x => new { x.source_index, x.destination_index });
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "admin",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'1', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    password_hash = table.Column<byte[]>(type: "bytea", nullable: true),
                    salt = table.Column<byte[]>(type: "bytea", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    role = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "reset_password_requests",
                schema: "admin",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reset_password_requests", x => x.id);
                    table.ForeignKey(
                        name: "fk_reset_password_requests_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "admin",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "admin",
                table: "users",
                columns: new[] { "id", "email", "password_hash", "role", "salt" },
                values: new object[] { 1, "admin.search@quantumart.ru", new byte[] { 94, 18, 53, 150, 210, 136, 179, 209, 156, 30, 64, 198, 133, 83, 109, 69, 84, 200, 79, 235, 26, 62, 142, 19, 160, 248, 86, 37, 32, 195, 97, 14 }, 1, new byte[] { 106, 117, 27, 162, 96, 145, 73, 17, 61, 117, 244, 203, 233, 110, 138, 112 } });

            migrationBuilder.CreateIndex(
                name: "ix_reset_password_requests_user_id",
                schema: "admin",
                table: "reset_password_requests",
                column: "user_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "reindex_tasks",
                schema: "admin");

            migrationBuilder.DropTable(
                name: "reset_password_requests",
                schema: "admin");

            migrationBuilder.DropTable(
                name: "users",
                schema: "admin");
        }
    }
}
