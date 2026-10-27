using Microsoft.EntityFrameworkCore;
using SkyPlus.API.DTOs.Reportes;
using SkyPlus.API.Exceptions;
using SkyPlus.Infrastructure.Data;

namespace SkyPlus.API.Services
{
    public class ReporteService : IReporteService
    {
        private readonly SkyPlusDbContext _context;

        public ReporteService(SkyPlusDbContext context)
        {
            _context = context;
        }

        public async Task<ReporteVentasResponse> ObtenerVentasAsync(
            DateTime desde,
            DateTime hasta)
        {
            desde = desde.Date;
            hasta = hasta.Date;

            if (desde > hasta)
            {
                throw new ValidacionNegocioException(
                    "La fecha inicial no puede ser posterior a la final.");
            }

            // Rango inclusivo de días: [desde, día posterior a hasta).
            if (hasta == DateTime.MaxValue.Date)
            {
                throw new ValidacionNegocioException(
                    "La fecha final no es válida.");
            }

            DateTime hastaExclusivo = hasta.AddDays(1);

            // JOIN entre Pago y Reserva.
            // Se contabilizan los pagos registrados.
            var pagos = await (
                from p in _context.Pagos.AsNoTracking()
                join r in _context.Reservas.AsNoTracking()
                    on p.IdReserva equals r.IdReserva
                where p.FechaPago >= desde
                   && p.FechaPago < hastaExclusivo
                   && p.EstadoPago == "Registrado"
                select new
                {
                    p.IdPago,
                    p.FechaPago,
                    p.Monto,
                    p.TipoPago,
                    r.CodigoReserva
                }
            ).ToListAsync();

            // Reembolsos registrados durante el período.
            // Se filtran por su propia fecha, no por fecha de pago.
            var reembolsos = await _context.Reembolsos
                .AsNoTracking()
                .Where(r =>
                    r.FechaReembolso >= desde &&
                    r.FechaReembolso < hastaExclusivo &&
                    r.EstadoReembolso == "Registrado")
                .Select(r => new
                {
                    r.FechaReembolso,
                    r.MontoReembolsado
                })
                .ToListAsync();

            decimal totalCobrado = pagos.Sum(p => p.Monto);
            decimal totalReembolsado =
                reembolsos.Sum(r => r.MontoReembolsado);

            // Agrupación diaria de pagos.
            var pagosPorDia = pagos
                .GroupBy(p => p.FechaPago.Date)
                .ToDictionary(
                    g => g.Key,
                    g => new
                    {
                        Cantidad = g.Count(),
                        Total = g.Sum(p => p.Monto)
                    });

            // Agrupación diaria de reembolsos.
            var reembolsosPorDia = reembolsos
                .GroupBy(r => r.FechaReembolso.Date)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(r => r.MontoReembolsado));

            // Incluir días con pagos o reembolsos.
            var fechas = pagosPorDia.Keys
                .Union(reembolsosPorDia.Keys)
                .OrderBy(f => f);

            var ventasPorDia = fechas.Select(fecha =>
            {
                pagosPorDia.TryGetValue(fecha, out var pagoDia);
                reembolsosPorDia.TryGetValue(
                    fecha, out decimal reembolsoDia);

                decimal cobradoDia = pagoDia?.Total ?? 0m;

                return new VentaDiariaResponse
                {
                    Fecha = fecha,
                    CantidadPagos = pagoDia?.Cantidad ?? 0,
                    TotalCobrado = cobradoDia,
                    TotalReembolsado = reembolsoDia,
                    IngresoNeto = cobradoDia - reembolsoDia
                };
            }).ToList();

            return new ReporteVentasResponse
            {
                Desde = desde,
                Hasta = hasta,

                CantidadPagos = pagos.Count,

                PagosIniciales = pagos.Count(p =>
                    p.TipoPago == "Inicial"),

                PagosAdicionales = pagos.Count(p =>
                    p.TipoPago == "Adicional"),

                TotalCobrado = totalCobrado,
                TotalReembolsado = totalReembolsado,
                IngresoNeto = totalCobrado - totalReembolsado,

                VentasPorDia = ventasPorDia
            };
        }

        public async Task<ReporteReservasResponse> ObtenerReservasAsync(
            DateTime desde,
            DateTime hasta)
        {
            desde = desde.Date;
            hasta = hasta.Date;

            if (desde > hasta)
            {
                throw new ValidacionNegocioException(
                    "La fecha inicial no puede ser posterior a la final.");
            }

            if (hasta == DateTime.MaxValue.Date)
            {
                throw new ValidacionNegocioException(
                    "La fecha final no es válida.");
            }

            DateTime hastaExclusivo = hasta.AddDays(1);

            // Obtener reservas creadas dentro del período.
            var reservas = await _context.Reservas
                .AsNoTracking()
                .Where(r =>
                    r.FechaReserva >= desde &&
                    r.FechaReserva < hastaExclusivo)
                .Select(r => new
                {
                    r.IdReserva,
                    r.EstadoReserva,
                    r.FechaReserva
                })
                .ToListAsync();

            int total = reservas.Count;

            // Agrupación por estado.
            var reservasPorEstado = reservas
                .GroupBy(r => r.EstadoReserva)
                .Select(g => new ReservaPorEstadoResponse
                {
                    Estado = g.Key,
                    Cantidad = g.Count(),
                    Porcentaje = total == 0
                        ? 0
                        : Math.Round(
                            (decimal)g.Count() * 100 / total, 2)
                })
                .OrderByDescending(r => r.Cantidad)
                .ToList();

            // Agrupación por fecha de creación.
            var reservasPorDia = reservas
                .GroupBy(r => r.FechaReserva.Date)
                .Select(g => new ReservaDiariaResponse
                {
                    Fecha = g.Key,
                    CantidadReservas = g.Count()
                })
                .OrderBy(r => r.Fecha)
                .ToList();

            return new ReporteReservasResponse
            {
                Desde = desde,
                Hasta = hasta,

                TotalReservas = total,

                Confirmadas = reservas.Count(r =>
                    r.EstadoReserva == "Confirmada"),

                PendientesPago = reservas.Count(r =>
                    r.EstadoReserva == "PendientePago"),

                Canceladas = reservas.Count(r =>
                    r.EstadoReserva == "Cancelada"),

                Expiradas = reservas.Count(r =>
                    r.EstadoReserva == "Expirada"),

                ReservasPorEstado = reservasPorEstado,
                ReservasPorDia = reservasPorDia
            };
        }

        public async Task<ReporteVuelosResponse> ObtenerVuelosAsync(
            DateTime desde,
            DateTime hasta)
        {
            desde = desde.Date;
            hasta = hasta.Date;

            if (desde > hasta)
                throw new ValidacionNegocioException(
                    "La fecha inicial no puede ser posterior a la final.");

            if (hasta == DateTime.MaxValue.Date)
                throw new ValidacionNegocioException(
                    "La fecha final no es válida.");

            DateTime hastaExclusivo = hasta.AddDays(1);

            // Vuelos cuya salida está dentro del período.
            var vuelos = await _context.Vuelos
                .AsNoTracking()
                .Where(v => v.Salida >= desde &&
                            v.Salida < hastaExclusivo)
                .Select(v => new
                {
                    v.IdVuelo,
                    v.NumeroVuelo,
                    v.Salida,
                    v.Llegada,
                    v.EstadoVuelo,
                    Origen = v.LugarOrigen!.CodigoIata,
                    Destino = v.LugarDestino!.CodigoIata
                })
                .OrderBy(v => v.Salida)
                .ToListAsync();

            var idsVuelos = vuelos.Select(v => v.IdVuelo).ToList();

            // Cantidad de asientos por vuelo.
            var asientos = await _context.Asientos
                .AsNoTracking()
                .Where(a => idsVuelos.Contains(a.IdVuelo))
                .GroupBy(a => a.IdVuelo)
                .Select(g => new
                {
                    IdVuelo = g.Key,
                    Total = g.Count(),
                    Ocupados = g.Count(a => a.Estado == "Ocupado")
                })
                .ToDictionaryAsync(a => a.IdVuelo);

            // Reservas confirmadas por vuelo.
            var reservasConfirmadas = await _context.Reservas
                .AsNoTracking()
                .Where(r => idsVuelos.Contains(r.IdVuelo) &&
                            r.EstadoReserva == "Confirmada")
                .GroupBy(r => r.IdVuelo)
                .Select(g => new
                {
                    IdVuelo = g.Key,
                    Cantidad = g.Count()
                })
                .ToDictionaryAsync(r => r.IdVuelo, r => r.Cantidad);

            var detalle = vuelos.Select(v =>
            {
                asientos.TryGetValue(v.IdVuelo, out var datosAsientos);
                reservasConfirmadas.TryGetValue(
                    v.IdVuelo, out int confirmadas);

                int total = datosAsientos?.Total ?? 0;
                int ocupados = datosAsientos?.Ocupados ?? 0;
                int disponibles = total - ocupados;

                return new VueloDetalleReporteResponse
                {
                    IdVuelo = v.IdVuelo,
                    NumeroVuelo = v.NumeroVuelo,
                    Origen = v.Origen,
                    Destino = v.Destino,
                    Salida = v.Salida,
                    Llegada = v.Llegada,
                    EstadoVuelo = v.EstadoVuelo,

                    TotalAsientos = total,
                    AsientosOcupados = ocupados,
                    AsientosDisponibles = disponibles,
                    ReservasConfirmadas = confirmadas,

                    PorcentajeOcupacion = total == 0
                        ? 0
                        : Math.Round(
                            (decimal)ocupados * 100 / total, 2)
                };
            }).ToList();

            int totalAsientos = detalle.Sum(v => v.TotalAsientos);
            int totalOcupados = detalle.Sum(v => v.AsientosOcupados);

            return new ReporteVuelosResponse
            {
                Desde = desde,
                Hasta = hasta,

                TotalVuelos = vuelos.Count,

                Programados = vuelos.Count(v =>
                    v.EstadoVuelo == "Programado"),

                Confirmados = vuelos.Count(v =>
                    v.EstadoVuelo == "Confirmado"),

                Cancelados = vuelos.Count(v =>
                    v.EstadoVuelo == "Cancelado"),

                Finalizados = vuelos.Count(v =>
                    v.EstadoVuelo == "Finalizado"),

                TotalAsientos = totalAsientos,
                AsientosOcupados = totalOcupados,
                AsientosDisponibles = totalAsientos - totalOcupados,

                PorcentajeOcupacionGeneral = totalAsientos == 0
                    ? 0
                    : Math.Round(
                        (decimal)totalOcupados * 100 / totalAsientos, 2),

                VuelosPorEstado = vuelos
                    .GroupBy(v => v.EstadoVuelo)
                    .Select(g => new VueloPorEstadoResponse
                    {
                        Estado = g.Key,
                        Cantidad = g.Count()
                    })
                    .OrderByDescending(v => v.Cantidad)
                    .ToList(),

                DetalleVuelos = detalle
            };
        }
        public async Task<ReportePasajerosResponse> ObtenerPasajerosAsync(
            DateTime desde,
            DateTime hasta)
        {
            desde = desde.Date;
            hasta = hasta.Date;

            if (desde > hasta)
                throw new ValidacionNegocioException(
                    "La fecha inicial no puede ser posterior a la final.");

            if (hasta == DateTime.MaxValue.Date)
                throw new ValidacionNegocioException(
                    "La fecha final no es válida.");

            DateTime hastaExclusivo = hasta.AddDays(1);

            // Total de pasajeros del sistema, sin filtro de fecha.
            int totalRegistrados = await _context.Pasajeros
                .AsNoTracking()
                .CountAsync();

            int activos = await _context.Pasajeros
                .AsNoTracking()
                .CountAsync(p => p.Estado == "Activo");

            int inactivos = await _context.Pasajeros
                .AsNoTracking()
                .CountAsync(p => p.Estado == "Inactivo");

            // Vuelos cuya salida está dentro del período.
            var vuelos = await _context.Vuelos
                .AsNoTracking()
                .Where(v =>
                    v.Salida >= desde &&
                    v.Salida < hastaExclusivo)
                .Select(v => new
                {
                    v.IdVuelo,
                    v.NumeroVuelo,
                    v.Salida
                })
                .ToListAsync();

            var idsVuelos = vuelos
                .Select(v => v.IdVuelo)
                .ToList();

            // JOIN entre Reserva y Pasajero.
            var reservas = await (
                from r in _context.Reservas.AsNoTracking()
                join p in _context.Pasajeros.AsNoTracking()
                    on r.IdPasajero equals p.IdPasajero
                where idsVuelos.Contains(r.IdVuelo)
                select new
                {
                    r.IdReserva,
                    r.IdVuelo,
                    r.IdPasajero,
                    r.EstadoReserva,
                    p.Nombre,
                    p.Apellido,
                    p.Documento
                }
            ).ToListAsync();

            // Reservas por vuelo.
            var pasajerosPorVuelo = vuelos
                .Select(v =>
                {
                    var reservasVuelo = reservas
                        .Where(r => r.IdVuelo == v.IdVuelo)
                        .ToList();

                    // Se excluyen canceladas y expiradas
                    // del conteo de pasajeros únicos.
                    var reservasVigentes = reservasVuelo
                        .Where(r =>
                            r.EstadoReserva == "Confirmada" ||
                            r.EstadoReserva == "PendientePago");

                    return new PasajeroPorVueloResponse
                    {
                        IdVuelo = v.IdVuelo,
                        NumeroVuelo = v.NumeroVuelo,
                        Salida = v.Salida,

                        TotalReservas = reservasVuelo.Count,

                        ReservasConfirmadas = reservasVuelo.Count(r =>
                            r.EstadoReserva == "Confirmada"),

                        PasajerosUnicos = reservasVigentes
                            .Select(r => r.IdPasajero)
                            .Distinct()
                            .Count()
                    };
                })
                .OrderBy(v => v.Salida)
                .ToList();

            // Pasajeros con más reservas en el período.
            var pasajerosFrecuentes = reservas
                .GroupBy(r => new
                {
                    r.IdPasajero,
                    r.Nombre,
                    r.Apellido,
                    r.Documento
                })
                .Select(g => new PasajeroFrecuenteResponse
                {
                    IdPasajero = g.Key.IdPasajero,
                    NombreCompleto =
                        $"{g.Key.Apellido}, {g.Key.Nombre}",
                    Documento = g.Key.Documento,

                    TotalReservas = g.Count(),

                    ReservasConfirmadas = g.Count(r =>
                        r.EstadoReserva == "Confirmada")
                })
                .OrderByDescending(p => p.ReservasConfirmadas)
                .ThenByDescending(p => p.TotalReservas)
                .Take(10)
                .ToList();

            int pasajerosConReservas = reservas
                .Select(r => r.IdPasajero)
                .Distinct()
                .Count();

            int pasajerosConfirmados = reservas
                .Where(r => r.EstadoReserva == "Confirmada")
                .Select(r => r.IdPasajero)
                .Distinct()
                .Count();

            return new ReportePasajerosResponse
            {
                Desde = desde,
                Hasta = hasta,

                TotalPasajerosRegistrados = totalRegistrados,
                PasajerosActivos = activos,
                PasajerosInactivos = inactivos,

                PasajerosConReservas = pasajerosConReservas,
                PasajerosConReservasConfirmadas =
                    pasajerosConfirmados,

                TotalReservasPeriodo = reservas.Count,

                PasajerosPorVuelo = pasajerosPorVuelo,
                PasajerosFrecuentes = pasajerosFrecuentes
            };
        }

        public async Task<ReporteEquipajesResponse> ObtenerEquipajesAsync(
            DateTime desde,
            DateTime hasta)
        {
            desde = desde.Date;
            hasta = hasta.Date;

            if (desde > hasta)
                throw new ValidacionNegocioException(
                    "La fecha inicial no puede ser posterior a la final.");

            if (hasta == DateTime.MaxValue.Date)
                throw new ValidacionNegocioException(
                    "La fecha final no es válida.");

            DateTime hastaExclusivo = hasta.AddDays(1);

            // JOIN entre equipajes contratados y tipos de equipaje.
            var equipajes = await (
                from e in _context.EquipajesReserva.AsNoTracking()
                join t in _context.TiposEquipaje.AsNoTracking()
                    on e.IdTipoEquipaje equals t.IdTipoEquipaje
                where e.FechaContratacion >= desde
                   && e.FechaContratacion < hastaExclusivo
                select new
                {
                    e.IdEquipajeReserva,
                    e.Estado,
                    e.PesoKg,
                    e.LimiteKgContratado,
                    e.Costo,
                    TipoEquipaje = t.Nombre
                }
            ).ToListAsync();

            // Cargos de equipaje y exceso de peso generados
            // dentro del período seleccionado.
            var cargos = await _context.CargosAdicionales
                .AsNoTracking()
                .Where(c =>
                    c.FechaCreacion >= desde &&
                    c.FechaCreacion < hastaExclusivo &&
                    (c.TipoCargo == "Equipaje" ||
                     c.TipoCargo == "ExcesoPeso"))
                .Select(c => new
                {
                    c.TipoCargo,
                    c.Estado,
                    c.Monto
                })
                .ToListAsync();

            // Equipajes agrupados por tipo.
            var equipajesPorTipo = equipajes
                .GroupBy(e => e.TipoEquipaje)
                .Select(g => new EquipajePorTipoResponse
                {
                    TipoEquipaje = g.Key,
                    Cantidad = g.Count(),

                    PesoTotalKg = g.Sum(e =>
                        e.PesoKg ?? 0m),

                    ImporteContratado = g.Sum(e =>
                        e.Estado == "Cancelado" ? 0m : e.Costo)
                })
                .OrderByDescending(e => e.Cantidad)
                .ToList();

            // Equipajes con peso superior al límite contratado.
            var equipajesConExceso = equipajes
                .Where(e =>
                    e.Estado != "Cancelado" &&
                    e.PesoKg.HasValue &&
                    e.PesoKg.Value > e.LimiteKgContratado)
                .ToList();

            decimal excesoTotalKg = equipajesConExceso
                .Sum(e => e.PesoKg!.Value - e.LimiteKgContratado);

            // Excluir cargos anulados del total exigible.
            var cargosVigentes = cargos
                .Where(c => c.Estado != "Anulado")
                .ToList();

            return new ReporteEquipajesResponse
            {
                Desde = desde,
                Hasta = hasta,

                TotalEquipajes = equipajes.Count,

                EquipajesActivos = equipajes.Count(e =>
                    e.Estado == "Activo"),

                EquipajesPendientesPago = equipajes.Count(e =>
                    e.Estado == "PendientePago"),

                EquipajesCancelados = equipajes.Count(e =>
                    e.Estado == "Cancelado"),

                EquipajesPesados = equipajes.Count(e =>
                    e.PesoKg.HasValue),

                EquipajesConExceso = equipajesConExceso.Count,

                PesoTotalKg = equipajes.Sum(e =>
                    e.PesoKg ?? 0m),

                ExcesoTotalKg = excesoTotalKg,

                ImporteContratado = equipajes
                    .Where(e => e.Estado != "Cancelado")
                    .Sum(e => e.Costo),

                CargosGenerados = cargosVigentes.Count,

                CargosPagados = cargosVigentes.Count(c =>
                    c.Estado == "Pagado"),

                CargosPendientes = cargosVigentes.Count(c =>
                    c.Estado == "Pendiente"),

                TotalCargosGenerados = cargosVigentes.Sum(c =>
                    c.Monto),

                TotalCargosPagados = cargosVigentes
                    .Where(c => c.Estado == "Pagado")
                    .Sum(c => c.Monto),

                TotalCargosPendientes = cargosVigentes
                    .Where(c => c.Estado == "Pendiente")
                    .Sum(c => c.Monto),

                EquipajesPorTipo = equipajesPorTipo
            };
        }

        public async Task<ReporteCheckInsResponse> ObtenerCheckInsAsync(
            DateTime desde,
            DateTime hasta)
        {
            desde = desde.Date;
            hasta = hasta.Date;

            if (desde > hasta)
                throw new ValidacionNegocioException(
                    "La fecha inicial no puede ser posterior a la final.");

            if (hasta == DateTime.MaxValue.Date)
                throw new ValidacionNegocioException(
                    "La fecha final no es válida.");

            DateTime hastaExclusivo = hasta.AddDays(1);

            // JOIN entre CheckIn, Reserva y Vuelo.
            // Filtramos por fecha de realización del check-in.
            var checkIns = await (
                from c in _context.CheckIns.AsNoTracking()
                join r in _context.Reservas.AsNoTracking()
                    on c.IdReserva equals r.IdReserva
                join v in _context.Vuelos.AsNoTracking()
                    on r.IdVuelo equals v.IdVuelo
                where c.FechaCheckin >= desde
                   && c.FechaCheckin < hastaExclusivo
                select new
                {
                    c.IdCheckin,
                    c.FechaCheckin,
                    c.EstadoCheckin,
                    v.IdVuelo,
                    v.NumeroVuelo,
                    v.Salida
                }
            ).ToListAsync();

            int total = checkIns.Count;

            int embarcados = checkIns.Count(c =>
                c.EstadoCheckin == "Embarcado");

            int pendientes = checkIns.Count(c =>
                c.EstadoCheckin == "Realizado");

            // Check-ins agrupados por día.
            var checkInsPorDia = checkIns
                .GroupBy(c => c.FechaCheckin.Date)
                .Select(g => new CheckInsPorDiaResponse
                {
                    Fecha = g.Key,
                    CantidadCheckIns = g.Count()
                })
                .OrderBy(c => c.Fecha)
                .ToList();

            // IDs de los vuelos que tuvieron check-ins
            // durante el período seleccionado.
            var idsVuelos = checkIns
                .Select(c => c.IdVuelo)
                .Distinct()
                .ToList();

            // Reservas confirmadas actuales de esos vuelos.
            var reservasConfirmadas = await _context.Reservas
                .AsNoTracking()
                .Where(r =>
                    idsVuelos.Contains(r.IdVuelo) &&
                    r.EstadoReserva == "Confirmada")
                .GroupBy(r => r.IdVuelo)
                .Select(g => new
                {
                    IdVuelo = g.Key,
                    Cantidad = g.Count()
                })
                .ToDictionaryAsync(
                    r => r.IdVuelo,
                    r => r.Cantidad);

            // Check-ins agrupados por vuelo.
            var checkInsPorVuelo = checkIns
                .GroupBy(c => new
                {
                    c.IdVuelo,
                    c.NumeroVuelo,
                    c.Salida
                })
                .Select(g =>
                {
                    int totalVuelo = g.Count();

                    int embarcadosVuelo = g.Count(c =>
                        c.EstadoCheckin == "Embarcado");

                    int pendientesVuelo = g.Count(c =>
                        c.EstadoCheckin == "Realizado");

                    reservasConfirmadas.TryGetValue(
                        g.Key.IdVuelo,
                        out int confirmadas);

                    return new CheckInsPorVueloResponse
                    {
                        IdVuelo = g.Key.IdVuelo,
                        NumeroVuelo = g.Key.NumeroVuelo,
                        Salida = g.Key.Salida,

                        ReservasConfirmadas = confirmadas,
                        TotalCheckIns = totalVuelo,
                        Embarcados = embarcadosVuelo,
                        PendientesEmbarque = pendientesVuelo,

                        PorcentajeEmbarque = totalVuelo == 0
                            ? 0
                            : Math.Round(
                                (decimal)embarcadosVuelo * 100 /
                                totalVuelo, 2)
                    };
                })
                .OrderBy(v => v.Salida)
                .ToList();

            return new ReporteCheckInsResponse
            {
                Desde = desde,
                Hasta = hasta,

                TotalCheckIns = total,
                Embarcados = embarcados,
                PendientesEmbarque = pendientes,

                PorcentajeEmbarque = total == 0
                    ? 0
                    : Math.Round(
                        (decimal)embarcados * 100 / total, 2),

                CheckInsPorVuelo = checkInsPorVuelo,
                CheckInsPorDia = checkInsPorDia
            };
        }
    }
}
