using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkyPlus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReservaExpiracionCincoMinutos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_expiracion",
                table: "reserva",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "fecha_expiracion",
                table: "reserva");
        }
    }
}
