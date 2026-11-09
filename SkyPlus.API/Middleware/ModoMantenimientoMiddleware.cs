using Microsoft.AspNetCore.Http;

namespace SkyPlus.API.Middleware
{
    public class ModoMantenimientoMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public ModoMantenimientoMiddleware(
            RequestDelegate next,
            IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string? rutaEstado = _configuration[
                "Mantenimiento:ArchivoEstado"];

            if (string.IsNullOrWhiteSpace(rutaEstado))
            {
                context.Response.StatusCode = 503;
                await context.Response.WriteAsJsonAsync(new
                {
                    mensaje = "No está configurado el estado de mantenimiento."
                });
                return;
            }

            // El archivo es una señal de mantenimiento.
            // Si existe, la API no procesa operaciones.
            if (File.Exists(rutaEstado))
            {
                context.Response.StatusCode =
                    StatusCodes.Status503ServiceUnavailable;

                context.Response.Headers.RetryAfter = "120";

                await context.Response.WriteAsJsonAsync(new
                {
                    mensaje = "SkyPlus se encuentra en mantenimiento.",
                    mantenimiento = true
                });

                return;
            }

            await _next(context);
        }
    }
}