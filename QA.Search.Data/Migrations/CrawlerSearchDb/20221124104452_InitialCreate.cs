using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace QA.Search.Data.Migrations.CrawlerSearchDb
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "crawler");

            migrationBuilder.CreateTable(
                name: "domain_groups",
                schema: "crawler",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    indexing_config = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_domain_groups", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "links",
                schema: "crawler",
                columns: table => new
                {
                    hash = table.Column<string>(type: "text", nullable: false),
                    url = table.Column<string>(type: "text", nullable: true),
                    next_indexing_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_links", x => x.hash);
                });

            migrationBuilder.CreateTable(
                name: "domains",
                schema: "crawler",
                columns: table => new
                {
                    origin = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    domain_group_id = table.Column<int>(type: "integer", nullable: false),
                    last_fast_crawling_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_deep_crawling_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_domains", x => x.origin);
                    table.ForeignKey(
                        name: "fk_domains_domain_groups_domain_group_id",
                        column: x => x.domain_group_id,
                        principalSchema: "crawler",
                        principalTable: "domain_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "routes",
                schema: "crawler",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    domain_group_id = table.Column<int>(type: "integer", nullable: false),
                    route = table.Column<string>(type: "text", nullable: true),
                    scan_period_msec = table.Column<int>(type: "integer", nullable: false),
                    indexing_config = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_routes", x => x.id);
                    table.ForeignKey(
                        name: "fk_routes_domain_groups_domain_group_id",
                        column: x => x.domain_group_id,
                        principalSchema: "crawler",
                        principalTable: "domain_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_domains_domain_group_id",
                schema: "crawler",
                table: "domains",
                column: "domain_group_id");

            migrationBuilder.CreateIndex(
                name: "ix_links_next_indexing_utc",
                schema: "crawler",
                table: "links",
                column: "next_indexing_utc")
                .Annotation("Npgsql:IndexInclude", new[] { "url", "version", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_routes_domain_group_id",
                schema: "crawler",
                table: "routes",
                column: "domain_group_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "domains",
                schema: "crawler");

            migrationBuilder.DropTable(
                name: "links",
                schema: "crawler");

            migrationBuilder.DropTable(
                name: "routes",
                schema: "crawler");

            migrationBuilder.DropTable(
                name: "domain_groups",
                schema: "crawler");
        }
    }
}
