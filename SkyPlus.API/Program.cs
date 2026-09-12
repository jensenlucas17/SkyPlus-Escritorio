using Microsoft.EntityFrameworkCore;
using SkyPlus.API.Services;
using SkyPlus.Infrastructure.Data;

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

// Servicios
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
