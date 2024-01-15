using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using FacturacionAPI1.Models;
using FacturacionAPI1.Repository;
using FacturacionAPI1.Repository.IRepositorio;
using System.Text.Json.Serialization;
using FacturacionAPI1;

var builder = WebApplication.CreateBuilder(args);

// Configuración de acceso a la configuración
var configuration = new ConfigurationBuilder()
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

// Registra los servicios en el contenedor.
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddAutoMapper(typeof(MappingConfig));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    });

// Configuración del contexto de la base de datos
builder.Services.AddDbContext<AplicationDbContext>(options =>
{
    options.UseSqlServer(configuration.GetConnectionString("Conexion"));
});

// Registra el repositorio
builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
builder.Services.AddScoped<IFacturaRepositorio, FacturaRepositorio>();
builder.Services.AddScoped<IProductoRepositorio, ProductoRepositorio>();
builder.Services.AddScoped<IDetalleFacturaRepositorio, DetalleFacturaRepositorio>();
builder.Services.AddScoped<IFamiliaProductoRepositorio, FamiliaProductoRepositorio>();

// Configuración de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("NuevaPolitica", app =>
    {
        app.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("NuevaPolitica");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
