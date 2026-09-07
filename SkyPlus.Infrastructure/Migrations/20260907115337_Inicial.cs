using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkyPlus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "aeronave",
                columns: table => new
                {
                    id_aeronave = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    matricula = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    modelo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aeronave", x => x.id_aeronave);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "lugar",
                columns: table => new
                {
                    id_lugar = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    codigo_iata = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nombre = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ciudad = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    pais = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lugar", x => x.id_lugar);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "pasajero",
                columns: table => new
                {
                    id_pasajero = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nombre = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    apellido = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    documento = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nacionalidad = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    telefono = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pasajero", x => x.id_pasajero);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "rol",
                columns: table => new
                {
                    id_rol = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nombre_rol = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rol", x => x.id_rol);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "tipo_equipaje",
                columns: table => new
                {
                    id_tipo_equipaje = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nombre = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    costo_adicional = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    limite_kg = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    descripcion = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipo_equipaje", x => x.id_tipo_equipaje);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "usuario",
                columns: table => new
                {
                    id_usuario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_rol = table.Column<int>(type: "int", nullable: false),
                    nombre = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    apellido = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email_corporativo = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    contrasena_hash = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    estado_cuenta = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ultima_sesion = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario", x => x.id_usuario);
                    table.ForeignKey(
                        name: "FK_usuario_rol_id_rol",
                        column: x => x.id_rol,
                        principalTable: "rol",
                        principalColumn: "id_rol",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "vuelo",
                columns: table => new
                {
                    id_vuelo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_aeronave = table.Column<int>(type: "int", nullable: false),
                    id_usuario_operador = table.Column<int>(type: "int", nullable: false),
                    id_lugar_origen = table.Column<int>(type: "int", nullable: false),
                    id_lugar_destino = table.Column<int>(type: "int", nullable: false),
                    numero_vuelo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    salida = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    llegada = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    estado_vuelo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tarifa = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vuelo", x => x.id_vuelo);
                    table.ForeignKey(
                        name: "FK_vuelo_aeronave_id_aeronave",
                        column: x => x.id_aeronave,
                        principalTable: "aeronave",
                        principalColumn: "id_aeronave",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_vuelo_lugar_id_lugar_destino",
                        column: x => x.id_lugar_destino,
                        principalTable: "lugar",
                        principalColumn: "id_lugar",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_vuelo_lugar_id_lugar_origen",
                        column: x => x.id_lugar_origen,
                        principalTable: "lugar",
                        principalColumn: "id_lugar",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_vuelo_usuario_id_usuario_operador",
                        column: x => x.id_usuario_operador,
                        principalTable: "usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "asiento",
                columns: table => new
                {
                    id_asiento = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_vuelo = table.Column<int>(type: "int", nullable: false),
                    fila = table.Column<int>(type: "int", nullable: false),
                    letra = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    estado = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asiento", x => x.id_asiento);
                    table.ForeignKey(
                        name: "FK_asiento_vuelo_id_vuelo",
                        column: x => x.id_vuelo,
                        principalTable: "vuelo",
                        principalColumn: "id_vuelo",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "reserva",
                columns: table => new
                {
                    id_reserva = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_pasajero = table.Column<int>(type: "int", nullable: false),
                    id_vuelo = table.Column<int>(type: "int", nullable: false),
                    id_asiento = table.Column<int>(type: "int", nullable: false),
                    id_usuario_agente = table.Column<int>(type: "int", nullable: false),
                    codigo_reserva = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tarifa_base = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    estado_reserva = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fecha_reserva = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    fecha_cancelacion = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reserva", x => x.id_reserva);
                    table.ForeignKey(
                        name: "FK_reserva_asiento_id_asiento",
                        column: x => x.id_asiento,
                        principalTable: "asiento",
                        principalColumn: "id_asiento",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reserva_pasajero_id_pasajero",
                        column: x => x.id_pasajero,
                        principalTable: "pasajero",
                        principalColumn: "id_pasajero",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reserva_usuario_id_usuario_agente",
                        column: x => x.id_usuario_agente,
                        principalTable: "usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reserva_vuelo_id_vuelo",
                        column: x => x.id_vuelo,
                        principalTable: "vuelo",
                        principalColumn: "id_vuelo",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "checkin",
                columns: table => new
                {
                    id_checkin = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_reserva = table.Column<int>(type: "int", nullable: false),
                    id_usuario_agente = table.Column<int>(type: "int", nullable: false),
                    codigo_qr = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    boarding_pass = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fecha_checkin = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_checkin", x => x.id_checkin);
                    table.ForeignKey(
                        name: "FK_checkin_reserva_id_reserva",
                        column: x => x.id_reserva,
                        principalTable: "reserva",
                        principalColumn: "id_reserva",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_checkin_usuario_id_usuario_agente",
                        column: x => x.id_usuario_agente,
                        principalTable: "usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "equipaje_reserva",
                columns: table => new
                {
                    id_equipaje_reserva = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_reserva = table.Column<int>(type: "int", nullable: false),
                    id_tipo_equipaje = table.Column<int>(type: "int", nullable: false),
                    peso_kg = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    costo = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_equipaje_reserva", x => x.id_equipaje_reserva);
                    table.ForeignKey(
                        name: "FK_equipaje_reserva_reserva_id_reserva",
                        column: x => x.id_reserva,
                        principalTable: "reserva",
                        principalColumn: "id_reserva",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_equipaje_reserva_tipo_equipaje_id_tipo_equipaje",
                        column: x => x.id_tipo_equipaje,
                        principalTable: "tipo_equipaje",
                        principalColumn: "id_tipo_equipaje",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "pago",
                columns: table => new
                {
                    id_pago = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_reserva = table.Column<int>(type: "int", nullable: false),
                    id_usuario_agente = table.Column<int>(type: "int", nullable: false),
                    monto = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    metodo_pago = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fecha_pago = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pago", x => x.id_pago);
                    table.ForeignKey(
                        name: "FK_pago_reserva_id_reserva",
                        column: x => x.id_reserva,
                        principalTable: "reserva",
                        principalColumn: "id_reserva",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pago_usuario_id_usuario_agente",
                        column: x => x.id_usuario_agente,
                        principalTable: "usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "reembolso",
                columns: table => new
                {
                    id_reembolso = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_pago = table.Column<int>(type: "int", nullable: false),
                    id_usuario_agente = table.Column<int>(type: "int", nullable: false),
                    monto_reembolsado = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    motivo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fecha_reembolso = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reembolso", x => x.id_reembolso);
                    table.ForeignKey(
                        name: "FK_reembolso_pago_id_pago",
                        column: x => x.id_pago,
                        principalTable: "pago",
                        principalColumn: "id_pago",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reembolso_usuario_id_usuario_agente",
                        column: x => x.id_usuario_agente,
                        principalTable: "usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_aeronave_matricula",
                table: "aeronave",
                column: "matricula",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_asiento_id_vuelo_fila_letra",
                table: "asiento",
                columns: new[] { "id_vuelo", "fila", "letra" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_checkin_id_reserva",
                table: "checkin",
                column: "id_reserva",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_checkin_id_usuario_agente",
                table: "checkin",
                column: "id_usuario_agente");

            migrationBuilder.CreateIndex(
                name: "IX_equipaje_reserva_id_reserva",
                table: "equipaje_reserva",
                column: "id_reserva");

            migrationBuilder.CreateIndex(
                name: "IX_equipaje_reserva_id_tipo_equipaje",
                table: "equipaje_reserva",
                column: "id_tipo_equipaje");

            migrationBuilder.CreateIndex(
                name: "IX_lugar_codigo_iata",
                table: "lugar",
                column: "codigo_iata",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pago_id_reserva",
                table: "pago",
                column: "id_reserva",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pago_id_usuario_agente",
                table: "pago",
                column: "id_usuario_agente");

            migrationBuilder.CreateIndex(
                name: "IX_pasajero_documento",
                table: "pasajero",
                column: "documento",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reembolso_id_pago",
                table: "reembolso",
                column: "id_pago",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reembolso_id_usuario_agente",
                table: "reembolso",
                column: "id_usuario_agente");

            migrationBuilder.CreateIndex(
                name: "IX_reserva_codigo_reserva",
                table: "reserva",
                column: "codigo_reserva",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reserva_id_asiento",
                table: "reserva",
                column: "id_asiento");

            migrationBuilder.CreateIndex(
                name: "IX_reserva_id_pasajero",
                table: "reserva",
                column: "id_pasajero");

            migrationBuilder.CreateIndex(
                name: "IX_reserva_id_usuario_agente",
                table: "reserva",
                column: "id_usuario_agente");

            migrationBuilder.CreateIndex(
                name: "IX_reserva_id_vuelo",
                table: "reserva",
                column: "id_vuelo");

            migrationBuilder.CreateIndex(
                name: "IX_rol_nombre_rol",
                table: "rol",
                column: "nombre_rol",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuario_email_corporativo",
                table: "usuario",
                column: "email_corporativo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuario_id_rol",
                table: "usuario",
                column: "id_rol");

            migrationBuilder.CreateIndex(
                name: "IX_vuelo_id_aeronave",
                table: "vuelo",
                column: "id_aeronave");

            migrationBuilder.CreateIndex(
                name: "IX_vuelo_id_lugar_destino",
                table: "vuelo",
                column: "id_lugar_destino");

            migrationBuilder.CreateIndex(
                name: "IX_vuelo_id_lugar_origen",
                table: "vuelo",
                column: "id_lugar_origen");

            migrationBuilder.CreateIndex(
                name: "IX_vuelo_id_usuario_operador",
                table: "vuelo",
                column: "id_usuario_operador");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "checkin");

            migrationBuilder.DropTable(
                name: "equipaje_reserva");

            migrationBuilder.DropTable(
                name: "reembolso");

            migrationBuilder.DropTable(
                name: "tipo_equipaje");

            migrationBuilder.DropTable(
                name: "pago");

            migrationBuilder.DropTable(
                name: "reserva");

            migrationBuilder.DropTable(
                name: "asiento");

            migrationBuilder.DropTable(
                name: "pasajero");

            migrationBuilder.DropTable(
                name: "vuelo");

            migrationBuilder.DropTable(
                name: "aeronave");

            migrationBuilder.DropTable(
                name: "lugar");

            migrationBuilder.DropTable(
                name: "usuario");

            migrationBuilder.DropTable(
                name: "rol");
        }
    }
}
