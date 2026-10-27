using Microsoft.EntityFrameworkCore;
using SkyPlus.API.Services;
using SkyPlus.Infrastructure.Data;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<SkyPlusDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("SkyPlusConnection"),
        ServerVersion.AutoDetect(
            builder.Configuration.GetConnectionString("SkyPlusConnection")
        )
    )
);

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<ILugarService, LugarService>();
builder.Services.AddScoped<IAeronaveService, AeronaveService>();
builder.Services.AddScoped<IVueloService, VueloService>();
builder.Services.AddScoped<IPasajeroService, PasajeroService>();
builder.Services.AddScoped<IAsientoService, AsientoService>();
builder.Services.AddScoped<IReservaService, ReservaService>();
builder.Services.AddScoped<IPagoService, PagoService>();
builder.Services.AddHostedService<ReservasExpirationService>();
builder.Services.AddScoped<IReembolsoService, ReembolsoService>();
builder.Services.AddScoped<IEquipajeService, EquipajeService>();
builder.Services.AddScoped<ICheckInService, CheckInService>();
builder.Services.AddScoped<BoardingPassService>();
builder.Services.AddScoped<IReporteService, ReporteService>();

// Servicios
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

QuestPDF.Settings.License = LicenseType.Community;

var app = builder.Build();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
