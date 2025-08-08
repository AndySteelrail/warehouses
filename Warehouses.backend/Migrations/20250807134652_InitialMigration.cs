using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Warehouses.backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CargoTypes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CargoTypes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Warehouses",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    closed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warehouses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Pickets",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    warehouse_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    closed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pickets", x => x.id);
                    table.ForeignKey(
                        name: "FK_Pickets_Warehouses_warehouse_id",
                        column: x => x.warehouse_id,
                        principalTable: "Warehouses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Platforms",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    closed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    warehouse_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Platforms", x => x.id);
                    table.ForeignKey(
                        name: "FK_Platforms_Warehouses_warehouse_id",
                        column: x => x.warehouse_id,
                        principalTable: "Warehouses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cargoes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    remainder = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    coming = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    consumption = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    recorded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    platform_id = table.Column<int>(type: "integer", nullable: false),
                    cargo_type_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cargoes", x => x.id);
                    table.ForeignKey(
                        name: "FK_Cargoes_CargoTypes_cargo_type_id",
                        column: x => x.cargo_type_id,
                        principalTable: "CargoTypes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Cargoes_Platforms_platform_id",
                        column: x => x.platform_id,
                        principalTable: "Platforms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlatformPickets",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    platform_id = table.Column<int>(type: "integer", nullable: false),
                    picket_id = table.Column<int>(type: "integer", nullable: false),
                    assigned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    unassigned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformPickets", x => x.id);
                    table.ForeignKey(
                        name: "FK_PlatformPickets_Pickets_picket_id",
                        column: x => x.picket_id,
                        principalTable: "Pickets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlatformPickets_Platforms_platform_id",
                        column: x => x.platform_id,
                        principalTable: "Platforms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cargoes_cargo_type_id",
                table: "Cargoes",
                column: "cargo_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_Cargoes_platform_id",
                table: "Cargoes",
                column: "platform_id");

            migrationBuilder.CreateIndex(
                name: "IX_Cargoes_recorded_at",
                table: "Cargoes",
                column: "recorded_at");

            migrationBuilder.CreateIndex(
                name: "IX_CargoTypes_name",
                table: "CargoTypes",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pickets_warehouse_id",
                table: "Pickets",
                column: "warehouse_id");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformPickets_picket_id",
                table: "PlatformPickets",
                column: "picket_id");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformPickets_platform_id",
                table: "PlatformPickets",
                column: "platform_id");

            migrationBuilder.CreateIndex(
                name: "IX_Platforms_created_at_closed_at",
                table: "Platforms",
                columns: new[] { "created_at", "closed_at" });

            migrationBuilder.CreateIndex(
                name: "IX_Platforms_warehouse_id",
                table: "Platforms",
                column: "warehouse_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cargoes");

            migrationBuilder.DropTable(
                name: "PlatformPickets");

            migrationBuilder.DropTable(
                name: "CargoTypes");

            migrationBuilder.DropTable(
                name: "Pickets");

            migrationBuilder.DropTable(
                name: "Platforms");

            migrationBuilder.DropTable(
                name: "Warehouses");
        }
    }
}
