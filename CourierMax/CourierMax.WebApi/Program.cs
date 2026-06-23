using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using CourierMax.Application.Services;
using CourierMax.Domain.Interfaces;
using CourierMax.Infrastructure.Data;
using CourierMax.Infrastructure.Repositories;
using CourierMax.WebApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddSimpleConsole(options => options.IncludeScopes = true);

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<CourierMaxDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IShipmentRepository, ShipmentRepository>();
builder.Services.AddScoped<IDriverRepository, DriverRepository>();
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<ICityDistanceRepository, CityDistanceRepository>();
builder.Services.AddScoped<ICostCalculationService, CostCalculationService>();
builder.Services.AddScoped<IShipmentService, ShipmentService>();
builder.Services.AddScoped<IDriverMetricsService, DriverMetricsService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CourierMaxDbContext>();
    db.Database.Migrate();
}

app.Run();
