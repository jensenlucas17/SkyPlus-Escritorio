namespace SkyPlus.Desktop.Models
{
    // ---------- Check-ins ----------
    public class CheckInsPorVueloResponse
    {
        public int IdVuelo { get; set; }
        public string NumeroVuelo { get; set; } = string.Empty;
        public DateTime Salida { get; set; }
        public int ReservasConfirmadas { get; set; }
        public int TotalCheckIns { get; set; }
        public int Embarcados { get; set; }
        public int PendientesEmbarque { get; set; }
        public decimal PorcentajeEmbarque { get; set; }
    }

    public class CheckInsPorDiaResponse
    {
        public DateTime Fecha { get; set; }
        public int CantidadCheckIns { get; set; }
    }

    public class ReporteCheckInsResponse
    {
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }
        public int TotalCheckIns { get; set; }
        public int Embarcados { get; set; }
        public int PendientesEmbarque { get; set; }
        public decimal PorcentajeEmbarque { get; set; }
        public List<CheckInsPorVueloResponse> CheckInsPorVuelo { get; set; } = new();
        public List<CheckInsPorDiaResponse> CheckInsPorDia { get; set; } = new();
    }

    // ---------- Equipajes ----------
    public class EquipajePorTipoResponse
    {
        public string TipoEquipaje { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PesoTotalKg { get; set; }
        public decimal ImporteContratado { get; set; }
    }

    public class ReporteEquipajesResponse
    {
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }
        public int TotalEquipajes { get; set; }
        public int EquipajesActivos { get; set; }
        public int EquipajesPendientesPago { get; set; }
        public int EquipajesCancelados { get; set; }
        public int EquipajesPesados { get; set; }
        public int EquipajesConExceso { get; set; }
        public decimal PesoTotalKg { get; set; }
        public decimal ExcesoTotalKg { get; set; }
        public decimal ImporteContratado { get; set; }
        public int CargosGenerados { get; set; }
        public int CargosPagados { get; set; }
        public int CargosPendientes { get; set; }
        public decimal TotalCargosGenerados { get; set; }
        public decimal TotalCargosPagados { get; set; }
        public decimal TotalCargosPendientes { get; set; }
        public List<EquipajePorTipoResponse> EquipajesPorTipo { get; set; } = new();
    }

    // ---------- Pasajeros ----------
    public class PasajeroPorVueloResponse
    {
        public int IdVuelo { get; set; }
        public string NumeroVuelo { get; set; } = string.Empty;
        public DateTime Salida { get; set; }
        public int TotalReservas { get; set; }
        public int ReservasConfirmadas { get; set; }
        public int PasajerosUnicos { get; set; }
    }

    public class PasajeroFrecuenteResponse
    {
        public int IdPasajero { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Documento { get; set; } = string.Empty;
        public int TotalReservas { get; set; }
        public int ReservasConfirmadas { get; set; }
    }

    public class ReportePasajerosResponse
    {
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }
        public int TotalPasajerosRegistrados { get; set; }
        public int PasajerosActivos { get; set; }
        public int PasajerosInactivos { get; set; }
        public int PasajerosConReservas { get; set; }
        public int PasajerosConReservasConfirmadas { get; set; }
        public int TotalReservasPeriodo { get; set; }
        public List<PasajeroPorVueloResponse> PasajerosPorVuelo { get; set; } = new();
        public List<PasajeroFrecuenteResponse> PasajerosFrecuentes { get; set; } = new();
    }

    // ---------- Reservas ----------
    public class ReservaDiariaResponse
    {
        public DateTime Fecha { get; set; }
        public int CantidadReservas { get; set; }
    }

    public class ReservaPorEstadoResponse
    {
        public string Estado { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal Porcentaje { get; set; }
    }

    public class ReporteReservasResponse
    {
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }
        public int TotalReservas { get; set; }
        public int Confirmadas { get; set; }
        public int PendientesPago { get; set; }
        public int Canceladas { get; set; }
        public int Expiradas { get; set; }
        public List<ReservaPorEstadoResponse> ReservasPorEstado { get; set; } = new();
        public List<ReservaDiariaResponse> ReservasPorDia { get; set; } = new();
    }

    // ---------- Ventas ----------
    public class VentaDiariaResponse
    {
        public DateTime Fecha { get; set; }
        public int CantidadPagos { get; set; }
        public decimal TotalCobrado { get; set; }
        public decimal TotalReembolsado { get; set; }
        public decimal IngresoNeto { get; set; }
    }

    public class ReporteVentasResponse
    {
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }
        public int CantidadPagos { get; set; }
        public int PagosIniciales { get; set; }
        public int PagosAdicionales { get; set; }
        public decimal TotalCobrado { get; set; }
        public decimal TotalReembolsado { get; set; }
        public decimal IngresoNeto { get; set; }
        public List<VentaDiariaResponse> VentasPorDia { get; set; } = new();
    }

    // ---------- Vuelos ----------
    public class VueloDetalleReporteResponse
    {
        public int IdVuelo { get; set; }
        public string NumeroVuelo { get; set; } = string.Empty;
        public string Origen { get; set; } = string.Empty;
        public string Destino { get; set; } = string.Empty;
        public DateTime Salida { get; set; }
        public DateTime Llegada { get; set; }
        public string EstadoVuelo { get; set; } = string.Empty;
        public int TotalAsientos { get; set; }
        public int AsientosOcupados { get; set; }
        public int AsientosDisponibles { get; set; }
        public int ReservasConfirmadas { get; set; }
        public decimal PorcentajeOcupacion { get; set; }
    }

    public class VueloPorEstadoResponse
    {
        public string Estado { get; set; } = string.Empty;
        public int Cantidad { get; set; }
    }

    public class ReporteVuelosResponse
    {
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }
        public int TotalVuelos { get; set; }
        public int Programados { get; set; }
        public int Confirmados { get; set; }
        public int Cancelados { get; set; }
        public int Finalizados { get; set; }
        public int TotalAsientos { get; set; }
        public int AsientosOcupados { get; set; }
        public int AsientosDisponibles { get; set; }
        public decimal PorcentajeOcupacionGeneral { get; set; }
        public List<VueloPorEstadoResponse> VuelosPorEstado { get; set; } = new();
        public List<VueloDetalleReporteResponse> DetalleVuelos { get; set; } = new();
    }
}