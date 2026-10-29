using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using QuestPDF.Infrastructure;
using SkyPlus.API.Services;
using SkyPlus.Infrastructure.Data;
using System.Security.Claims;

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
builder.Services.AddSingleton<PasswordService>();
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
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Ingresá el token JWT.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecuritySchemeReference(
                    "Bearer",
                    document
                ),
                new List<string>()
            }
        });
});

QuestPDF.Settings.License = LicenseType.Community;


builder.Services.AddScoped<JwtTokenService>();

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "Falta configurar Jwt:Key.");

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException(
        "Falta configurar Jwt:Issuer.");

var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException(
        "Falta configurar Jwt:Audience.");

var signingKey = new SymmetricSecurityKey(
    Convert.FromBase64String(jwtKey));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = true;

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,

                ValidateAudience = true,
                ValidAudience = jwtAudience,

                ValidateLifetime = true,
                RequireExpirationTime = true,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,

                ClockSkew = TimeSpan.Zero
            };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var idClaim = context.Principal?.FindFirst(
                    ClaimTypes.NameIdentifier)?.Value;

                if (!int.TryParse(idClaim, out int idUsuario))
                {
                    context.Fail("Token sin identificador válido.");
                    return;
                }

                var db = context.HttpContext.RequestServices
                    .GetRequiredService<SkyPlusDbContext>();

                var usuario = await db.Usuarios
                    .AsNoTracking()
                    .Include(u => u.Rol)
                    .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

                if (usuario == null ||
                    usuario.EstadoCuenta != "Activo" ||
                    usuario.Rol == null)
                {
                    context.Fail("Usuario inexistente o inactivo.");
                    return;
                }

                var rolToken = context.Principal?.FindFirst(
                    ClaimTypes.Role)?.Value;

                if (rolToken != usuario.Rol.NombreRol)
                {
                    context.Fail("El rol del usuario cambió.");
                    return;
                }
            }
        };

    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
