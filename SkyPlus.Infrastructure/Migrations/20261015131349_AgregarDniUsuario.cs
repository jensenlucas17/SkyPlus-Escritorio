using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkyPlus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarDniUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "dni",
                table: "usuario",
                type: "varchar(8)",
                maxLength: 8,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_dni",
                table: "usuario",
                column: "dni",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_usuario_dni",
                table: "usuario");

            migrationBuilder.DropColumn(
                name: "dni",
                table: "usuario");
        }
    }
}
