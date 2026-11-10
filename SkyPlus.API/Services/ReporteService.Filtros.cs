
using Microsoft.EntityFrameworkCore;
using SkyPlus.API.DTOs.Reportes;
using SkyPlus.API.Exceptions;
using SkyPlus.Domain.Entities;

namespace SkyPlus.API.Services
{
    public partial class ReporteService
    {
        // =========================================
        // VALIDACIONES Y MÉTODOS AUXILIARES
        // =========================================

        private static void ValidarFiltros(ReporteFiltrosRequest f)
        {
            if (f == null)
                throw new ValidacionNegocioException(
                    "Los filtros son obligatorios.");

            if (f.Desde.Date > f.Hasta.Date ||
                f.Hasta.Date == DateTime.MaxValue.Date)
                throw new ValidacionNegocioException(
                    "El período seleccionado no es válido.");

            if (f.Agrupacion != "Dia" &&
                f.Agrupacion != "Semana" &&
                f.Agrupacion != "Mes")
                throw new ValidacionNegocioException(
                    "La agrupación debe ser Dia, Semana o Mes.");

            if (f.CompararPeriodoAnterior)
                throw new ValidacionNegocioException(
                    "La comparación entre períodos aún no está implementada.");
        }

        private static void ValidarFiltrosOperativos(
            ReporteFiltrosRequest f)
        {
            ValidarFiltros(f);

            if (!string.IsNullOrWhiteSpace(f.MetodoPago) ||
                !string.IsNullOrWhiteSpace(f.TipoPago))
                throw new ValidacionNegocioException(
                    "Los filtros de pago corresponden al reporte de ventas.");
        }

        private static DateTime AgruparFecha(
            DateTime fecha, string agrupacion)
        {
            fecha = fecha.Date;

            return agrupacion switch
            {
                "Mes" => new DateTime(
                    fecha.Year, fecha.Month, 1),

                "Semana" => fecha.AddDays(
                    -((int)fecha.DayOfWeek + 6) % 7),

                _ => fecha
            };
        }

        private static bool TieneFiltrosAvanzados(
            ReporteFiltrosRequest f)
        {
            return f.IdVuelo.HasValue ||
                   f.IdLugarOrigen.HasValue ||
                   f.IdLugarDestino.HasValue ||
                   !string.IsNullOrWhiteSpace(f.EstadoVuelo) ||
                   !string.IsNullOrWhiteSpace(f.EstadoReserva) ||
                   !string.IsNullOrWhiteSpace(f.MetodoPago) ||
                   !string.IsNullOrWhiteSpace(f.TipoPago);
        }

        private IQueryable<Vuelo> FiltrarVuelos(
            ReporteFiltrosRequest f)
        {
            var query = _context.Vuelos.AsNoTracking();

            if (f.IdVuelo.HasValue)
                query = query.Where(v =>
                    v.IdVuelo == f.IdVuelo.Value);

            if (f.IdLugarOrigen.HasValue)
                query = query.Where(v =>
                    v.IdLugarOrigen == f.IdLugarOrigen.Value);

            if (f.IdLugarDestino.HasValue)
                query = query.Where(v =>
                    v.IdLugarDestino == f.IdLugarDestino.Value);

            if (!string.IsNullOrWhiteSpace(f.EstadoVuelo))
                query = query.Where(v =>
                    v.EstadoVuelo == f.EstadoVuelo);

            return query;
        }

        // =========================================
        // REPORTE DE VENTAS
        // =========================================

        public async Task<ReporteVentasResponse> ObtenerVentasAsync(
            ReporteFiltrosRequest f)
        {
            ValidarFiltros(f);

            if (!TieneFiltrosAvanzados(f) &&
                f.Agrupacion == "Dia")
                return await ObtenerVentasAsync(f.Desde, f.Hasta);

            DateTime desde = f.Desde.Date;
            DateTime hasta = f.Hasta.Date;
            DateTime fin = hasta.AddDays(1);

            var pagosQuery =
                from p in _context.Pagos.AsNoTracking()
                join r in _context.Reservas.AsNoTracking()
                    on p.IdReserva equals r.IdReserva
                join v in _context.Vuelos.AsNoTracking()
                    on r.IdVuelo equals v.IdVuelo
                where p.FechaPago >= desde &&
                      p.FechaPago < fin &&
                      p.EstadoPago == "Registrado"
                select new { p, r, v };

            if (f.IdVuelo.HasValue)
                pagosQuery = pagosQuery.Where(x =>
                    x.v.IdVuelo == f.IdVuelo.Value);

            if (f.IdLugarOrigen.HasValue)
                pagosQuery = pagosQuery.Where(x =>
                    x.v.IdLugarOrigen == f.IdLugarOrigen.Value);

            if (f.IdLugarDestino.HasValue)
                pagosQuery = pagosQuery.Where(x =>
                    x.v.IdLugarDestino == f.IdLugarDestino.Value);

            if (!string.IsNullOrWhiteSpace(f.EstadoVuelo))
                pagosQuery = pagosQuery.Where(x =>
                    x.v.EstadoVuelo == f.EstadoVuelo);

            if (!string.IsNullOrWhiteSpace(f.EstadoReserva))
                pagosQuery = pagosQuery.Where(x =>
                    x.r.EstadoReserva == f.EstadoReserva);

            if (!string.IsNullOrWhiteSpace(f.MetodoPago))
                pagosQuery = pagosQuery.Where(x =>
                    x.p.MetodoPago == f.MetodoPago);

            if (!string.IsNullOrWhiteSpace(f.TipoPago))
                pagosQuery = pagosQuery.Where(x =>
                    x.p.TipoPago == f.TipoPago);

            var pagos = await pagosQuery
                .Select(x => new
                {
                    x.p.FechaPago,
                    x.p.Monto,
                    x.p.TipoPago
                })
                .ToListAsync();

            var reembolsosQuery =
                from re in _context.Reembolsos.AsNoTracking()
                join p in _context.Pagos.AsNoTracking()
                    on re.IdPago equals p.IdPago
                join r in _context.Reservas.AsNoTracking()
                    on p.IdReserva equals r.IdReserva
                join v in _context.Vuelos.AsNoTracking()
                    on r.IdVuelo equals v.IdVuelo
                where re.FechaReembolso >= desde &&
                      re.FechaReembolso < fin &&
                      re.EstadoReembolso == "Registrado"
                select new { re, p, r, v };

            if (f.IdVuelo.HasValue)
                reembolsosQuery = reembolsosQuery.Where(x =>
                    x.v.IdVuelo == f.IdVuelo.Value);

            if (f.IdLugarOrigen.HasValue)
                reembolsosQuery = reembolsosQuery.Where(x =>
                    x.v.IdLugarOrigen == f.IdLugarOrigen.Value);

            if (f.IdLugarDestino.HasValue)
                reembolsosQuery = reembolsosQuery.Where(x =>
                    x.v.IdLugarDestino == f.IdLugarDestino.Value);

            if (!string.IsNullOrWhiteSpace(f.EstadoVuelo))
                reembolsosQuery = reembolsosQuery.Where(x =>
                    x.v.EstadoVuelo == f.EstadoVuelo);

            if (!string.IsNullOrWhiteSpace(f.EstadoReserva))
                reembolsosQuery = reembolsosQuery.Where(x =>
                    x.r.EstadoReserva == f.EstadoReserva);

            if (!string.IsNullOrWhiteSpace(f.MetodoPago))
                reembolsosQuery = reembolsosQuery.Where(x =>
                    x.p.MetodoPago == f.MetodoPago);

            if (!string.IsNullOrWhiteSpace(f.TipoPago))
                reembolsosQuery = reembolsosQuery.Where(x =>
                    x.p.TipoPago == f.TipoPago);

            var reembolsos = await reembolsosQuery
                .Select(x => new
                {
                    x.re.FechaReembolso,
                    x.re.MontoReembolsado
                })
                .ToListAsync();

            var pagosAgrupados = pagos
                .GroupBy(p => AgruparFecha(
                    p.FechaPago, f.Agrupacion))
                .ToDictionary(
                    g => g.Key,
                    g => new
                    {
                        Cantidad = g.Count(),
                        Monto = g.Sum(p => p.Monto)
                    });

            var reembolsosAgrupados = reembolsos
                .GroupBy(r => AgruparFecha(
                    r.FechaReembolso, f.Agrupacion))
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(r => r.MontoReembolsado));

            var fechas = pagosAgrupados.Keys
                .Union(reembolsosAgrupados.Keys)
                .OrderBy(x => x);

            var detalle = fechas.Select(fecha =>
            {
                pagosAgrupados.TryGetValue(
                    fecha, out var datosPago);

                reembolsosAgrupados.TryGetValue(
                    fecha, out decimal reembolsado);

                decimal cobrado = datosPago?.Monto ?? 0m;

                return new VentaDiariaResponse
                {
                    Fecha = fecha,
                    CantidadPagos = datosPago?.Cantidad ?? 0,
                    TotalCobrado = cobrado,
                    TotalReembolsado = reembolsado,
                    IngresoNeto = cobrado - reembolsado
                };
            }).ToList();

            decimal totalCobrado = pagos.Sum(x => x.Monto);
            decimal totalReembolsado =
                reembolsos.Sum(x => x.MontoReembolsado);

            return new ReporteVentasResponse
            {
                Desde = desde,
                Hasta = hasta,
                CantidadPagos = pagos.Count,
                PagosIniciales = pagos.Count(x =>
                    x.TipoPago == "Inicial"),
                PagosAdicionales = pagos.Count(x =>
                    x.TipoPago == "Adicional"),
                TotalCobrado = totalCobrado,
                TotalReembolsado = totalReembolsado,
                IngresoNeto = totalCobrado - totalReembolsado,
                VentasPorDia = detalle
            };
        }

        // =========================================
        // REPORTE DE RESERVAS
        // =========================================

        public async Task<ReporteReservasResponse> ObtenerReservasAsync(
            ReporteFiltrosRequest f)
        {
            ValidarFiltros(f);

            if (!TieneFiltrosAvanzados(f) &&
                f.Agrupacion == "Dia")
                return await ObtenerReservasAsync(f.Desde, f.Hasta);

            if (!string.IsNullOrWhiteSpace(f.MetodoPago) ||
                !string.IsNullOrWhiteSpace(f.TipoPago))
                throw new ValidacionNegocioException(
                    "Los filtros de pago corresponden al reporte de ventas.");

            DateTime desde = f.Desde.Date;
            DateTime hasta = f.Hasta.Date;
            DateTime fin = hasta.AddDays(1);

            var query =
                from r in _context.Reservas.AsNoTracking()
                join v in _context.Vuelos.AsNoTracking()
                    on r.IdVuelo equals v.IdVuelo
                where r.FechaReserva >= desde &&
                      r.FechaReserva < fin
                select new { r, v };

            if (f.IdVuelo.HasValue)
                query = query.Where(x =>
                    x.v.IdVuelo == f.IdVuelo.Value);

            if (f.IdLugarOrigen.HasValue)
                query = query.Where(x =>
                    x.v.IdLugarOrigen == f.IdLugarOrigen.Value);

            if (f.IdLugarDestino.HasValue)
                query = query.Where(x =>
                    x.v.IdLugarDestino == f.IdLugarDestino.Value);

            if (!string.IsNullOrWhiteSpace(f.EstadoVuelo))
                query = query.Where(x =>
                    x.v.EstadoVuelo == f.EstadoVuelo);

            if (!string.IsNullOrWhiteSpace(f.EstadoReserva))
                query = query.Where(x =>
                    x.r.EstadoReserva == f.EstadoReserva);

            var reservas = await query
                .Select(x => new
                {
                    x.r.FechaReserva,
                    x.r.EstadoReserva
                })
                .ToListAsync();

            int total = reservas.Count;

            var porEstado = reservas
                .GroupBy(x => x.EstadoReserva)
                .Select(g => new ReservaPorEstadoResponse
                {
                    Estado = g.Key,
                    Cantidad = g.Count(),
                    Porcentaje = total == 0
                        ? 0m
                        : Math.Round(
                            (decimal)g.Count() * 100 / total, 2)
                })
                .OrderByDescending(x => x.Cantidad)
                .ToList();

            var porFecha = reservas
                .GroupBy(x => AgruparFecha(
                    x.FechaReserva, f.Agrupacion))
                .Select(g => new ReservaDiariaResponse
                {
                    Fecha = g.Key,
                    CantidadReservas = g.Count()
                })
                .OrderBy(x => x.Fecha)
                .ToList();

            return new ReporteReservasResponse
            {
                Desde = desde,
                Hasta = hasta,
                TotalReservas = total,
                Confirmadas = reservas.Count(x =>
                    x.EstadoReserva == "Confirmada"),
                PendientesPago = reservas.Count(x =>
                    x.EstadoReserva == "PendientePago"),
                Canceladas = reservas.Count(x =>
                    x.EstadoReserva == "Cancelada"),
                Expiradas = reservas.Count(x =>
                    x.EstadoReserva == "Expirada"),
                ReservasPorEstado = porEstado,
                ReservasPorDia = porFecha
            };
        }

        // =========================================
        // REPORTE DE VUELOS
        // =========================================

        private async Task<ReporteVuelosResponse> ObtenerVuelosPeriodoAsync(
            ReporteFiltrosRequest f)
        {
            ValidarFiltrosOperativos(f);

            if (!string.IsNullOrWhiteSpace(f.EstadoReserva))
                throw new ValidacionNegocioException(
                    "El filtro de estado de reserva no corresponde al reporte de vuelos.");

            DateTime desde = f.Desde.Date;
            DateTime hasta = f.Hasta.Date;
            DateTime fin = hasta.AddDays(1);

            var vuelos = await FiltrarVuelos(f)
                .Where(v => v.Salida >= desde &&
                            v.Salida < fin)
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

            var ids = vuelos.Select(v => v.IdVuelo).ToList();

            var asientos = await _context.Asientos
                .AsNoTracking()
                .Where(a => ids.Contains(a.IdVuelo))
                .GroupBy(a => a.IdVuelo)
                .Select(g => new
                {
                    IdVuelo = g.Key,
                    Total = g.Count(),
                    Ocupados = g.Count(a =>
                        a.Estado == "Ocupado")
                })
                .ToDictionaryAsync(x => x.IdVuelo);

            var confirmadas = await _context.Reservas
                .AsNoTracking()
                .Where(r => ids.Contains(r.IdVuelo) &&
                            r.EstadoReserva == "Confirmada")
                .GroupBy(r => r.IdVuelo)
                .Select(g => new
                {
                    IdVuelo = g.Key,
                    Cantidad = g.Count()
                })
                .ToDictionaryAsync(
                    x => x.IdVuelo,
                    x => x.Cantidad);

            var detalle = vuelos.Select(v =>
            {
                asientos.TryGetValue(v.IdVuelo, out var a);
                confirmadas.TryGetValue(
                    v.IdVuelo, out int reservasConfirmadas);

                int total = a?.Total ?? 0;
                int ocupados = a?.Ocupados ?? 0;

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
                    AsientosDisponibles = total - ocupados,
                    ReservasConfirmadas = reservasConfirmadas,
                    PorcentajeOcupacion = total == 0
                        ? 0
                        : Math.Round(
                            (decimal)ocupados * 100 / total, 2)
                };
            }).ToList();

            int totalAsientos =
                detalle.Sum(x => x.TotalAsientos);

            int totalOcupados =
                detalle.Sum(x => x.AsientosOcupados);

            var rankingRutas = detalle
                .GroupBy(v => new
                {
                    v.Origen,
                    v.Destino
                })
                .Select(g =>
                {
                    int totalAsientos = g.Sum(x => x.TotalAsientos);
                    int ocupados = g.Sum(x => x.AsientosOcupados);

                    return new RutaRankingResponse
                    {
                        Origen = g.Key.Origen,
                        Destino = g.Key.Destino,

                        CantidadVuelos = g.Count(),

                        ReservasConfirmadas =
                            g.Sum(x => x.ReservasConfirmadas),

                        TotalAsientos = totalAsientos,
                        AsientosOcupados = ocupados,

                        PorcentajeOcupacion = totalAsientos == 0
                            ? 0m
                            : Math.Round(
                                (decimal)ocupados * 100 / totalAsientos,
                                2)
                    };
                })
                .OrderByDescending(x => x.CantidadVuelos)
                .ThenByDescending(x => x.ReservasConfirmadas)
                .ThenBy(x => x.Origen)
                .ThenBy(x => x.Destino)
                .Take(10)
                .ToList();

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
                AsientosDisponibles =
                    totalAsientos - totalOcupados,
                PorcentajeOcupacionGeneral =
                    totalAsientos == 0
                        ? 0
                        : Math.Round(
                            (decimal)totalOcupados * 100 /
                            totalAsientos, 2),
                VuelosPorEstado = vuelos
                    .GroupBy(v => v.EstadoVuelo)
                    .Select(g => new VueloPorEstadoResponse
                    {
                        Estado = g.Key,
                        Cantidad = g.Count()
                    })
                    .OrderByDescending(x => x.Cantidad)
                    .ToList(),
                DetalleVuelos = detalle,
                RankingRutas = rankingRutas
            };
        }

        // =========================================
        // REPORTE DE PASAJEROS
        // =========================================

        public async Task<ReportePasajerosResponse> ObtenerPasajerosAsync(
            ReporteFiltrosRequest f)
        {
            ValidarFiltrosOperativos(f);

            DateTime desde = f.Desde.Date;
            DateTime hasta = f.Hasta.Date;
            DateTime fin = hasta.AddDays(1);

            int registrados = await _context.Pasajeros
                .AsNoTracking().CountAsync();

            int activos = await _context.Pasajeros
                .AsNoTracking()
                .CountAsync(p => p.Estado == "Activo");

            int inactivos = await _context.Pasajeros
                .AsNoTracking()
                .CountAsync(p => p.Estado == "Inactivo");

            var vuelos = await FiltrarVuelos(f)
                .Where(v => v.Salida >= desde &&
                            v.Salida < fin)
                .Select(v => new
                {
                    v.IdVuelo,
                    v.NumeroVuelo,
                    v.Salida
                })
                .ToListAsync();

            var ids = vuelos.Select(v => v.IdVuelo).ToList();

            var query =
                from r in _context.Reservas.AsNoTracking()
                join p in _context.Pasajeros.AsNoTracking()
                    on r.IdPasajero equals p.IdPasajero
                where ids.Contains(r.IdVuelo)
                select new
                {
                    r.IdVuelo,
                    r.IdPasajero,
                    r.EstadoReserva,
                    p.Nombre,
                    p.Apellido,
                    p.Documento
                };

            if (!string.IsNullOrWhiteSpace(f.EstadoReserva))
                query = query.Where(r =>
                    r.EstadoReserva == f.EstadoReserva);

            var reservas = await query.ToListAsync();

            var porVuelo = vuelos.Select(v =>
            {
                var rv = reservas
                    .Where(r => r.IdVuelo == v.IdVuelo)
                    .ToList();

                return new PasajeroPorVueloResponse
                {
                    IdVuelo = v.IdVuelo,
                    NumeroVuelo = v.NumeroVuelo,
                    Salida = v.Salida,
                    TotalReservas = rv.Count,
                    ReservasConfirmadas = rv.Count(r =>
                        r.EstadoReserva == "Confirmada"),
                    PasajerosUnicos = rv
                        .Where(r =>
                            r.EstadoReserva == "Confirmada" ||
                            r.EstadoReserva == "PendientePago")
                        .Select(r => r.IdPasajero)
                        .Distinct()
                        .Count()
                };
            })
            .OrderBy(x => x.Salida)
            .ToList();

            var frecuentes = reservas
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
                .OrderByDescending(x =>
                    x.ReservasConfirmadas)
                .ThenByDescending(x => x.TotalReservas)
                .Take(10)
                .ToList();

            return new ReportePasajerosResponse
            {
                Desde = desde,
                Hasta = hasta,
                TotalPasajerosRegistrados = registrados,
                PasajerosActivos = activos,
                PasajerosInactivos = inactivos,
                PasajerosConReservas = reservas
                    .Select(r => r.IdPasajero)
                    .Distinct().Count(),
                PasajerosConReservasConfirmadas = reservas
                    .Where(r =>
                        r.EstadoReserva == "Confirmada")
                    .Select(r => r.IdPasajero)
                    .Distinct().Count(),
                TotalReservasPeriodo = reservas.Count,
                PasajerosPorVuelo = porVuelo,
                PasajerosFrecuentes = frecuentes
            };
        }

        // =========================================
        // REPORTE DE EQUIPAJES
        // =========================================

        public async Task<ReporteEquipajesResponse> ObtenerEquipajesAsync(
            ReporteFiltrosRequest f)
        {
            ValidarFiltrosOperativos(f);

            DateTime desde = f.Desde.Date;
            DateTime hasta = f.Hasta.Date;
            DateTime fin = hasta.AddDays(1);

            var idsVuelos = FiltrarVuelos(f)
                .Select(v => v.IdVuelo);

            var equipajesQuery =
                from e in _context.EquipajesReserva.AsNoTracking()
                join r in _context.Reservas.AsNoTracking()
                    on e.IdReserva equals r.IdReserva
                join t in _context.TiposEquipaje.AsNoTracking()
                    on e.IdTipoEquipaje equals t.IdTipoEquipaje
                where e.FechaContratacion >= desde &&
                      e.FechaContratacion < fin &&
                      idsVuelos.Contains(r.IdVuelo)
                select new { e, r, t };

            if (!string.IsNullOrWhiteSpace(f.EstadoReserva))
                equipajesQuery = equipajesQuery.Where(x =>
                    x.r.EstadoReserva == f.EstadoReserva);

            var equipajes = await equipajesQuery
                .Select(x => new
                {
                    x.e.Estado,
                    x.e.PesoKg,
                    x.e.LimiteKgContratado,
                    x.e.Costo,
                    Tipo = x.t.Nombre
                })
                .ToListAsync();

            var cargosQuery =
                from c in _context.CargosAdicionales.AsNoTracking()
                join r in _context.Reservas.AsNoTracking()
                    on c.IdReserva equals r.IdReserva
                where c.FechaCreacion >= desde &&
                      c.FechaCreacion < fin &&
                      idsVuelos.Contains(r.IdVuelo) &&
                      (c.TipoCargo == "Equipaje" ||
                       c.TipoCargo == "ExcesoPeso")
                select new { c, r };

            if (!string.IsNullOrWhiteSpace(f.EstadoReserva))
                cargosQuery = cargosQuery.Where(x =>
                    x.r.EstadoReserva == f.EstadoReserva);

            var cargos = await cargosQuery
                .Where(x => x.c.Estado != "Anulado")
                .Select(x => new
                {
                    x.c.Estado,
                    x.c.Monto
                })
                .ToListAsync();

            var exceso = equipajes
                .Where(e =>
                    e.Estado != "Cancelado" &&
                    e.PesoKg.HasValue &&
                    e.PesoKg.Value > e.LimiteKgContratado)
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
                EquipajesConExceso = exceso.Count,
                PesoTotalKg = equipajes.Sum(e =>
                    e.PesoKg ?? 0m),
                ExcesoTotalKg = exceso.Sum(e =>
                    e.PesoKg!.Value - e.LimiteKgContratado),
                ImporteContratado = equipajes
                    .Where(e => e.Estado != "Cancelado")
                    .Sum(e => e.Costo),
                CargosGenerados = cargos.Count,
                CargosPagados = cargos.Count(c =>
                    c.Estado == "Pagado"),
                CargosPendientes = cargos.Count(c =>
                    c.Estado == "Pendiente"),
                TotalCargosGenerados = cargos.Sum(c =>
                    c.Monto),
                TotalCargosPagados = cargos
                    .Where(c => c.Estado == "Pagado")
                    .Sum(c => c.Monto),
                TotalCargosPendientes = cargos
                    .Where(c => c.Estado == "Pendiente")
                    .Sum(c => c.Monto),
                EquipajesPorTipo = equipajes
                    .GroupBy(e => e.Tipo)
                    .Select(g => new EquipajePorTipoResponse
                    {
                        TipoEquipaje = g.Key,
                        Cantidad = g.Count(),
                        PesoTotalKg = g.Sum(e =>
                            e.PesoKg ?? 0m),
                        ImporteContratado = g
                            .Where(e =>
                                e.Estado != "Cancelado")
                            .Sum(e => e.Costo)
                    })
                    .OrderByDescending(x => x.Cantidad)
                    .ToList()
            };
        }

        // =========================================
        // REPORTE DE CHECK-INS
        // =========================================

        public async Task<ReporteCheckInsResponse> ObtenerCheckInsAsync(
            ReporteFiltrosRequest f)
        {
            ValidarFiltrosOperativos(f);

            DateTime desde = f.Desde.Date;
            DateTime hasta = f.Hasta.Date;
            DateTime fin = hasta.AddDays(1);

            var idsVuelos = FiltrarVuelos(f)
                .Select(v => v.IdVuelo);

            var query =
                from c in _context.CheckIns.AsNoTracking()
                join r in _context.Reservas.AsNoTracking()
                    on c.IdReserva equals r.IdReserva
                join v in _context.Vuelos.AsNoTracking()
                    on r.IdVuelo equals v.IdVuelo
                where c.FechaCheckin >= desde &&
                      c.FechaCheckin < fin &&
                      idsVuelos.Contains(v.IdVuelo)
                select new { c, r, v };

            if (!string.IsNullOrWhiteSpace(f.EstadoReserva))
                query = query.Where(x =>
                    x.r.EstadoReserva == f.EstadoReserva);

            var checkIns = await query
                .Select(x => new
                {
                    x.c.FechaCheckin,
                    x.c.EstadoCheckin,
                    x.v.IdVuelo,
                    x.v.NumeroVuelo,
                    x.v.Salida
                })
                .ToListAsync();

            var ids = checkIns
                .Select(x => x.IdVuelo)
                .Distinct()
                .ToList();

            var confirmadasQuery = _context.Reservas
                .AsNoTracking()
                .Where(r =>
                    ids.Contains(r.IdVuelo) &&
                    r.EstadoReserva == "Confirmada");

            if (!string.IsNullOrWhiteSpace(f.EstadoReserva) &&
                f.EstadoReserva != "Confirmada")
                confirmadasQuery = confirmadasQuery.Where(r =>
                    false);

            var confirmadas = await confirmadasQuery
                .GroupBy(r => r.IdVuelo)
                .Select(g => new
                {
                    IdVuelo = g.Key,
                    Cantidad = g.Count()
                })
                .ToDictionaryAsync(
                    x => x.IdVuelo,
                    x => x.Cantidad);

            var porVuelo = checkIns
                .GroupBy(x => new
                {
                    x.IdVuelo,
                    x.NumeroVuelo,
                    x.Salida
                })
                .Select(g =>
                {
                    confirmadas.TryGetValue(
                        g.Key.IdVuelo, out int cantidad);

                    int total = g.Count();
                    int embarcados = g.Count(x =>
                        x.EstadoCheckin == "Embarcado");

                    return new CheckInsPorVueloResponse
                    {
                        IdVuelo = g.Key.IdVuelo,
                        NumeroVuelo = g.Key.NumeroVuelo,
                        Salida = g.Key.Salida,
                        ReservasConfirmadas = cantidad,
                        TotalCheckIns = total,
                        Embarcados = embarcados,
                        PendientesEmbarque = g.Count(x =>
                            x.EstadoCheckin == "Realizado"),
                        PorcentajeEmbarque = total == 0
                            ? 0
                            : Math.Round(
                                (decimal)embarcados * 100 /
                                total, 2)
                    };
                })
                .OrderBy(x => x.Salida)
                .ToList();

            int totalGeneral = checkIns.Count;
            int totalEmbarcados = checkIns.Count(x =>
                x.EstadoCheckin == "Embarcado");

            return new ReporteCheckInsResponse
            {
                Desde = desde,
                Hasta = hasta,
                TotalCheckIns = totalGeneral,
                Embarcados = totalEmbarcados,
                PendientesEmbarque = checkIns.Count(x =>
                    x.EstadoCheckin == "Realizado"),
                PorcentajeEmbarque = totalGeneral == 0
                    ? 0
                    : Math.Round(
                        (decimal)totalEmbarcados * 100 /
                        totalGeneral, 2),
                CheckInsPorVuelo = porVuelo,
                CheckInsPorDia = checkIns
                    .GroupBy(x => AgruparFecha(
                        x.FechaCheckin, f.Agrupacion))
                    .Select(g => new CheckInsPorDiaResponse
                    {
                        Fecha = g.Key,
                        CantidadCheckIns = g.Count()
                    })
                    .OrderBy(x => x.Fecha)
                    .ToList()
            };
        }
    }
}
