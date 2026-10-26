
using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkyPlus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class IncorporarEquipajeYCargos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // MariaDB necesita un índice sobre id_reserva para sostener la FK.
            // Crear un índice auxiliar antes de quitar la unicidad anterior.
            migrationBuilder.CreateIndex(
                name: "IX_pago_id_reserva_aux",
                table: "pago",
                column: "id_reserva");

            // Eliminar el índice único anterior manteniendo el índice auxiliar.
            migrationBuilder.DropIndex(
                name: "IX_pago_id_reserva",
                table: "pago");

            migrationBuilder.AddColumn<string>(
                name: "tipo_pago",
                table: "pago",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Inicial")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<decimal>(
                name: "peso_kg",
                table: "equipaje_reserva",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "estado",
                table: "equipaje_reserva",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "PendientePago",
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldMaxLength: 10,
                oldDefaultValue: "Activo")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_contratacion",
                table: "equipaje_reserva",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "limite_kg_contratado",
                table: "equipaje_reserva",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "cargo_adicional",
                columns: table => new
                {
                    id_cargo_adicional = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_reserva = table.Column<int>(type: "int", nullable: false),
                    id_equipaje_reserva = table.Column<int>(type: "int", nullable: true),
                    tipo_cargo = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    descripcion = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    monto = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    estado = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "Pendiente")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fecha_creacion = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cargo_adicional", x => x.id_cargo_adicional);
                    table.ForeignKey(
                        name: "FK_cargo_adicional_equipaje_reserva_id_equipaje_reserva",
                        column: x => x.id_equipaje_reserva,
                        principalTable: "equipaje_reserva",
                        principalColumn: "id_equipaje_reserva",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_cargo_adicional_reserva_id_reserva",
                        column: x => x.id_reserva,
                        principalTable: "reserva",
                        principalColumn: "id_reserva",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "pago_detalle",
                columns: table => new
                {
                    id_pago_detalle = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_pago = table.Column<int>(type: "int", nullable: false),
                    id_cargo_adicional = table.Column<int>(type: "int", nullable: true),
                    concepto = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    monto = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pago_detalle", x => x.id_pago_detalle);
                    table.ForeignKey(
                        name: "FK_pago_detalle_cargo_adicional_id_cargo_adicional",
                        column: x => x.id_cargo_adicional,
                        principalTable: "cargo_adicional",
                        principalColumn: "id_cargo_adicional",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pago_detalle_pago_id_pago",
                        column: x => x.id_pago,
                        principalTable: "pago",
                        principalColumn: "id_pago",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            // Reemplazar el índice auxiliar por el índice definitivo no único.
            migrationBuilder.CreateIndex(
                name: "IX_pago_id_reserva",
                table: "pago",
                column: "id_reserva");

            migrationBuilder.DropIndex(
                name: "IX_pago_id_reserva_aux",
                table: "pago");

            migrationBuilder.CreateIndex(
                name: "IX_cargo_adicional_id_equipaje_reserva",
                table: "cargo_adicional",
                column: "id_equipaje_reserva");

            migrationBuilder.CreateIndex(
                name: "IX_cargo_adicional_id_reserva",
                table: "cargo_adicional",
                column: "id_reserva");

            migrationBuilder.CreateIndex(
                name: "IX_pago_detalle_id_cargo_adicional",
                table: "pago_detalle",
                column: "id_cargo_adicional",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pago_detalle_id_pago",
                table: "pago_detalle",
                column: "id_pago");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pago_detalle");

            migrationBuilder.DropTable(
                name: "cargo_adicional");

            migrationBuilder.CreateIndex(
                name: "IX_pago_id_reserva_aux",
                table: "pago",
                column: "id_reserva");

            migrationBuilder.DropIndex(
                name: "IX_pago_id_reserva",
                table: "pago");

            migrationBuilder.DropColumn(
                name: "tipo_pago",
                table: "pago");

            migrationBuilder.DropColumn(
                name: "fecha_contratacion",
                table: "equipaje_reserva");

            migrationBuilder.DropColumn(
                name: "limite_kg_contratado",
                table: "equipaje_reserva");

            migrationBuilder.AlterColumn<decimal>(
                name: "peso_kg",
                table: "equipaje_reserva",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "estado",
                table: "equipaje_reserva",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "Activo",
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "PendientePago")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_pago_id_reserva",
                table: "pago",
                column: "id_reserva",
                unique: true);

            migrationBuilder.DropIndex(
                name: "IX_pago_id_reserva_aux",
                table: "pago");
        }
    }
}
