using Microsoft.EntityFrameworkCore;
using SkyPlus.Domain.Entities;

namespace SkyPlus.Infrastructure.Data
{
    public class SkyPlusDbContext : DbContext
    {
        public SkyPlusDbContext(DbContextOptions<SkyPlusDbContext> options)
            : base(options)
        {
        }

        // Tablas
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Pasajero> Pasajeros { get; set; }
        public DbSet<Aeronave> Aeronaves { get; set; }
        public DbSet<Lugar> Lugares { get; set; }
        public DbSet<Vuelo> Vuelos { get; set; }
        public DbSet<Asiento> Asientos { get; set; }
        public DbSet<TipoEquipaje> TiposEquipaje { get; set; }
        public DbSet<Reserva> Reservas { get; set; }
        public DbSet<EquipajeReserva> EquipajesReserva { get; set; }
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<Reembolso> Reembolsos { get; set; }
        public DbSet<CheckIn> CheckIns { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =====================================================
            // ROL
            // =====================================================

            modelBuilder.Entity<Rol>(entity =>
            {
                entity.ToTable("rol");

                entity.HasKey(e => e.IdRol);

                entity.Property(e => e.IdRol)
                    .HasColumnName("id_rol");

                entity.Property(e => e.NombreRol)
                    .HasColumnName("nombre_rol")
                    .IsRequired();

                entity.HasIndex(e => e.NombreRol)
                    .IsUnique();
            });


            // =====================================================
            // USUARIO
            // =====================================================

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("usuario");

                entity.HasKey(e => e.IdUsuario);

                entity.Property(e => e.IdUsuario)
                    .HasColumnName("id_usuario");

                entity.Property(e => e.IdRol)
                    .HasColumnName("id_rol")
                    .IsRequired();

                entity.Property(e => e.Nombre)
                    .HasColumnName("nombre")
                    .IsRequired();

                entity.Property(e => e.Apellido)
                    .HasColumnName("apellido")
                    .IsRequired();

                entity.Property(e => e.EmailCorporativo)
                    .HasColumnName("email_corporativo")
                    .IsRequired();

                entity.Property(e => e.ContrasenaHash)
                    .HasColumnName("contrasena_hash")
                    .IsRequired();

                entity.Property(e => e.EstadoCuenta)
                    .HasColumnName("estado_cuenta")
                    .IsRequired();

                entity.Property(e => e.UltimaSesion)
                    .HasColumnName("ultima_sesion");

                entity.HasIndex(e => e.EmailCorporativo)
                    .IsUnique();

                entity.HasOne(e => e.Rol)
                    .WithMany()
                    .HasForeignKey(e => e.IdRol)
                    .OnDelete(DeleteBehavior.Restrict);
            });


            // =====================================================
            // PASAJERO
            // =====================================================

            modelBuilder.Entity<Pasajero>(entity =>
            {
                entity.ToTable("pasajero");

                entity.HasKey(e => e.IdPasajero);

                entity.Property(e => e.IdPasajero)
                    .HasColumnName("id_pasajero");

                entity.Property(e => e.Nombre)
                    .HasColumnName("nombre")
                    .IsRequired();

                entity.Property(e => e.Apellido)
                    .HasColumnName("apellido")
                    .IsRequired();

                entity.Property(e => e.Documento)
                    .HasColumnName("documento")
                    .IsRequired();

                entity.Property(e => e.Nacionalidad)
                    .HasColumnName("nacionalidad")
                    .IsRequired();

                entity.Property(e => e.Email)
                    .HasColumnName("email")
                    .IsRequired();

                entity.Property(e => e.Telefono)
                    .HasColumnName("telefono")
                    .IsRequired();

                entity.HasIndex(e => e.Documento)
                    .IsUnique();
            });


            // =====================================================
            // AERONAVE
            // =====================================================

            modelBuilder.Entity<Aeronave>(entity =>
            {
                entity.ToTable("aeronave");

                entity.HasKey(e => e.IdAeronave);

                entity.Property(e => e.IdAeronave)
                    .HasColumnName("id_aeronave");

                entity.Property(e => e.Matricula)
                    .HasColumnName("matricula")
                    .IsRequired();

                entity.Property(e => e.Modelo)
                    .HasColumnName("modelo")
                    .IsRequired();

                entity.HasIndex(e => e.Matricula)
                    .IsUnique();
            });


            // =====================================================
            // LUGAR
            // =====================================================

            modelBuilder.Entity<Lugar>(entity =>
            {
                entity.ToTable("lugar");

                entity.HasKey(e => e.IdLugar);

                entity.Property(e => e.IdLugar)
                    .HasColumnName("id_lugar");

                entity.Property(e => e.CodigoIata)
                    .HasColumnName("codigo_iata")
                    .IsRequired();

                entity.Property(e => e.Nombre)
                    .HasColumnName("nombre")
                    .IsRequired();

                entity.Property(e => e.Ciudad)
                    .HasColumnName("ciudad")
                    .IsRequired();

                entity.Property(e => e.Pais)
                    .HasColumnName("pais")
                    .IsRequired();

                entity.HasIndex(e => e.CodigoIata)
                    .IsUnique();
            });


            // =====================================================
            // VUELO
            // =====================================================

            modelBuilder.Entity<Vuelo>(entity =>
            {
                entity.ToTable("vuelo");

                entity.HasKey(e => e.IdVuelo);

                entity.Property(e => e.IdVuelo)
                    .HasColumnName("id_vuelo");

                entity.Property(e => e.IdAeronave)
                    .HasColumnName("id_aeronave")
                    .IsRequired();

                entity.Property(e => e.IdUsuarioOperador)
                    .HasColumnName("id_usuario_operador")
                    .IsRequired();

                entity.Property(e => e.IdLugarOrigen)
                    .HasColumnName("id_lugar_origen")
                    .IsRequired();

                entity.Property(e => e.IdLugarDestino)
                    .HasColumnName("id_lugar_destino")
                    .IsRequired();

                entity.Property(e => e.NumeroVuelo)
                    .HasColumnName("numero_vuelo")
                    .IsRequired();

                entity.Property(e => e.Salida)
                    .HasColumnName("salida")
                    .IsRequired();

                entity.Property(e => e.Llegada)
                    .HasColumnName("llegada")
                    .IsRequired();

                entity.Property(e => e.EstadoVuelo)
                    .HasColumnName("estado_vuelo")
                    .IsRequired();

                entity.Property(e => e.Tarifa)
                    .HasColumnName("tarifa")
                    .HasPrecision(10, 2)
                    .IsRequired();

                // Aeronave → Vuelos
                entity.HasOne(e => e.Aeronave)
                    .WithMany()
                    .HasForeignKey(e => e.IdAeronave)
                    .OnDelete(DeleteBehavior.Restrict);

                // Usuario → Vuelos
                entity.HasOne(e => e.UsuarioOperador)
                    .WithMany()
                    .HasForeignKey(e => e.IdUsuarioOperador)
                    .OnDelete(DeleteBehavior.Restrict);

                // Lugar origen → Vuelos
                entity.HasOne(e => e.LugarOrigen)
                    .WithMany()
                    .HasForeignKey(e => e.IdLugarOrigen)
                    .OnDelete(DeleteBehavior.Restrict);

                // Lugar destino → Vuelos
                entity.HasOne(e => e.LugarDestino)
                    .WithMany()
                    .HasForeignKey(e => e.IdLugarDestino)
                    .OnDelete(DeleteBehavior.Restrict);
            });


            // =====================================================
            // ASIENTO
            // =====================================================

            modelBuilder.Entity<Asiento>(entity =>
            {
                entity.ToTable("asiento");

                entity.HasKey(e => e.IdAsiento);

                entity.Property(e => e.IdAsiento)
                    .HasColumnName("id_asiento");

                entity.Property(e => e.IdVuelo)
                    .HasColumnName("id_vuelo")
                    .IsRequired();

                entity.Property(e => e.Fila)
                    .HasColumnName("fila")
                    .IsRequired();

                entity.Property(e => e.Letra)
                    .HasColumnName("letra")
                    .IsRequired();

                entity.Property(e => e.Estado)
                    .HasColumnName("estado")
                    .IsRequired();

                // Un asiento no puede repetirse dentro del mismo vuelo
                entity.HasIndex(e => new
                {
                    e.IdVuelo,
                    e.Fila,
                    e.Letra
                }).IsUnique();

                entity.HasOne(e => e.Vuelo)
                    .WithMany()
                    .HasForeignKey(e => e.IdVuelo)
                    .OnDelete(DeleteBehavior.Restrict);
            });


            // =====================================================
            // TIPO EQUIPAJE
            // =====================================================

            modelBuilder.Entity<TipoEquipaje>(entity =>
            {
                entity.ToTable("tipo_equipaje");

                entity.HasKey(e => e.IdTipoEquipaje);

                entity.Property(e => e.IdTipoEquipaje)
                    .HasColumnName("id_tipo_equipaje");

                entity.Property(e => e.Nombre)
                    .HasColumnName("nombre")
                    .IsRequired();

                entity.Property(e => e.CostoAdicional)
                    .HasColumnName("costo_adicional")
                    .HasPrecision(10, 2)
                    .IsRequired();

                entity.Property(e => e.LimiteKg)
                    .HasColumnName("limite_kg")
                    .HasPrecision(10, 2)
                    .IsRequired();

                entity.Property(e => e.Descripcion)
                    .HasColumnName("descripcion")
                    .IsRequired();
            });


            // =====================================================
            // RESERVA
            // =====================================================

            modelBuilder.Entity<Reserva>(entity =>
            {
                entity.ToTable("reserva");

                entity.HasKey(e => e.IdReserva);

                entity.Property(e => e.IdReserva)
                    .HasColumnName("id_reserva");

                entity.Property(e => e.IdPasajero)
                    .HasColumnName("id_pasajero")
                    .IsRequired();

                entity.Property(e => e.IdVuelo)
                    .HasColumnName("id_vuelo")
                    .IsRequired();

                entity.Property(e => e.IdAsiento)
                    .HasColumnName("id_asiento")
                    .IsRequired();

                entity.Property(e => e.IdUsuarioAgente)
                    .HasColumnName("id_usuario_agente")
                    .IsRequired();

                entity.Property(e => e.CodigoReserva)
                    .HasColumnName("codigo_reserva")
                    .IsRequired();

                entity.Property(e => e.TarifaBase)
                    .HasColumnName("tarifa_base")
                    .HasPrecision(10, 2)
                    .IsRequired();

                entity.Property(e => e.EstadoReserva)
                    .HasColumnName("estado_reserva")
                    .IsRequired();

                entity.Property(e => e.FechaReserva)
                    .HasColumnName("fecha_reserva")
                    .IsRequired();

                entity.Property(e => e.FechaCancelacion)
                    .HasColumnName("fecha_cancelacion");

                entity.HasIndex(e => e.CodigoReserva)
                    .IsUnique();

                // Pasajero → Reservas
                entity.HasOne(e => e.Pasajero)
                    .WithMany()
                    .HasForeignKey(e => e.IdPasajero)
                    .OnDelete(DeleteBehavior.Restrict);

                // Vuelo → Reservas
                entity.HasOne(e => e.Vuelo)
                    .WithMany()
                    .HasForeignKey(e => e.IdVuelo)
                    .OnDelete(DeleteBehavior.Restrict);

                // Asiento → Reservas
                entity.HasOne(e => e.Asiento)
                    .WithMany()
                    .HasForeignKey(e => e.IdAsiento)
                    .OnDelete(DeleteBehavior.Restrict);

                // Usuario → Reservas
                entity.HasOne(e => e.UsuarioAgente)
                    .WithMany()
                    .HasForeignKey(e => e.IdUsuarioAgente)
                    .OnDelete(DeleteBehavior.Restrict);
            });


            // =====================================================
            // EQUIPAJE RESERVA
            // =====================================================

            modelBuilder.Entity<EquipajeReserva>(entity =>
            {
                entity.ToTable("equipaje_reserva");

                entity.HasKey(e => e.IdEquipajeReserva);

                entity.Property(e => e.IdEquipajeReserva)
                    .HasColumnName("id_equipaje_reserva");

                entity.Property(e => e.IdReserva)
                    .HasColumnName("id_reserva")
                    .IsRequired();

                entity.Property(e => e.IdTipoEquipaje)
                    .HasColumnName("id_tipo_equipaje")
                    .IsRequired();

                entity.Property(e => e.PesoKg)
                    .HasColumnName("peso_kg")
                    .HasPrecision(10, 2)
                    .IsRequired();

                entity.Property(e => e.Costo)
                    .HasColumnName("costo")
                    .HasPrecision(10, 2)
                    .IsRequired();

                entity.HasOne(e => e.Reserva)
                    .WithMany()
                    .HasForeignKey(e => e.IdReserva)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.TipoEquipaje)
                    .WithMany()
                    .HasForeignKey(e => e.IdTipoEquipaje)
                    .OnDelete(DeleteBehavior.Restrict);
            });


            // =====================================================
            // PAGO
            // =====================================================

            modelBuilder.Entity<Pago>(entity =>
            {
                entity.ToTable("pago");

                entity.HasKey(e => e.IdPago);

                entity.Property(e => e.IdPago)
                    .HasColumnName("id_pago");

                entity.Property(e => e.IdReserva)
                    .HasColumnName("id_reserva")
                    .IsRequired();

                entity.Property(e => e.IdUsuarioAgente)
                    .HasColumnName("id_usuario_agente")
                    .IsRequired();

                entity.Property(e => e.Monto)
                    .HasColumnName("monto")
                    .HasPrecision(10, 2)
                    .IsRequired();

                entity.Property(e => e.MetodoPago)
                    .HasColumnName("metodo_pago")
                    .IsRequired();

                entity.Property(e => e.FechaPago)
                    .HasColumnName("fecha_pago")
                    .IsRequired();

                // Una reserva tiene un pago
                entity.HasIndex(e => e.IdReserva)
                    .IsUnique();

                entity.HasOne(e => e.Reserva)
                    .WithMany()
                    .HasForeignKey(e => e.IdReserva)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.UsuarioAgente)
                    .WithMany()
                    .HasForeignKey(e => e.IdUsuarioAgente)
                    .OnDelete(DeleteBehavior.Restrict);
            });


            // =====================================================
            // REEMBOLSO
            // =====================================================

            modelBuilder.Entity<Reembolso>(entity =>
            {
                entity.ToTable("reembolso");

                entity.HasKey(e => e.IdReembolso);

                entity.Property(e => e.IdReembolso)
                    .HasColumnName("id_reembolso");

                entity.Property(e => e.IdPago)
                    .HasColumnName("id_pago")
                    .IsRequired();

                entity.Property(e => e.IdUsuarioAgente)
                    .HasColumnName("id_usuario_agente")
                    .IsRequired();

                entity.Property(e => e.MontoReembolsado)
                    .HasColumnName("monto_reembolsado")
                    .HasPrecision(10, 2)
                    .IsRequired();

                entity.Property(e => e.Motivo)
                    .HasColumnName("motivo")
                    .IsRequired();

                entity.Property(e => e.FechaReembolso)
                    .HasColumnName("fecha_reembolso")
                    .IsRequired();

                // Un pago no debería tener más de un reembolso
                entity.HasIndex(e => e.IdPago)
                    .IsUnique();

                entity.HasOne(e => e.Pago)
                    .WithMany()
                    .HasForeignKey(e => e.IdPago)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.UsuarioAgente)
                    .WithMany()
                    .HasForeignKey(e => e.IdUsuarioAgente)
                    .OnDelete(DeleteBehavior.Restrict);
            });


            // =====================================================
            // CHECK-IN
            // =====================================================

            modelBuilder.Entity<CheckIn>(entity =>
            {
                entity.ToTable("checkin");

                entity.HasKey(e => e.IdCheckin);

                entity.Property(e => e.IdCheckin)
                    .HasColumnName("id_checkin");

                entity.Property(e => e.IdReserva)
                    .HasColumnName("id_reserva")
                    .IsRequired();

                entity.Property(e => e.IdUsuarioAgente)
                    .HasColumnName("id_usuario_agente")
                    .IsRequired();

                entity.Property(e => e.CodigoQr)
                    .HasColumnName("codigo_qr")
                    .IsRequired();

                entity.Property(e => e.BoardingPass)
                    .HasColumnName("boarding_pass")
                    .IsRequired();

                entity.Property(e => e.FechaCheckin)
                    .HasColumnName("fecha_checkin")
                    .IsRequired();

                // Una reserva no puede tener más de un check-in
                entity.HasIndex(e => e.IdReserva)
                    .IsUnique();

                entity.HasOne(e => e.Reserva)
                    .WithMany()
                    .HasForeignKey(e => e.IdReserva)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.UsuarioAgente)
                    .WithMany()
                    .HasForeignKey(e => e.IdUsuarioAgente)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}