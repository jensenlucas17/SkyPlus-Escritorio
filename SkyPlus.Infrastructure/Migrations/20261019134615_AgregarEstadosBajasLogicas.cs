using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkyPlus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarEstadosBajasLogicas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "estado",
                table: "tipo_equipaje",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "Activo")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "estado",
                table: "rol",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "Activo")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "estado_reembolso",
                table: "reembolso",
                type: "varchar(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "Registrado")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "estado",
                table: "pasajero",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "Activo")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "estado_pago",
                table: "pago",
                type: "varchar(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "Registrado")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "estado",
                table: "lugar",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "Activo")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "estado",
                table: "equipaje_reserva",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "Activo")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "estado_checkin",
                table: "checkin",
                type: "varchar(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "Realizado")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "estado",
                table: "aeronave",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "Activo")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "estado",
                table: "tipo_equipaje");

            migrationBuilder.DropColumn(
                name: "estado",
                table: "rol");

            migrationBuilder.DropColumn(
                name: "estado_reembolso",
                table: "reembolso");

            migrationBuilder.DropColumn(
                name: "estado",
                table: "pasajero");

            migrationBuilder.DropColumn(
                name: "estado_pago",
                table: "pago");

            migrationBuilder.DropColumn(
                name: "estado",
                table: "lugar");

            migrationBuilder.DropColumn(
                name: "estado",
                table: "equipaje_reserva");

            migrationBuilder.DropColumn(
                name: "estado_checkin",
                table: "checkin");

            migrationBuilder.DropColumn(
                name: "estado",
                table: "aeronave");
        }
    }
}
