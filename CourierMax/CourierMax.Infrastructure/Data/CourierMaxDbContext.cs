using Microsoft.EntityFrameworkCore;
using CourierMax.Domain.Entities;
using CourierMax.Domain.Enums;
using CourierMax.Domain.ValueObjects;

namespace CourierMax.Infrastructure.Data;

public class CourierMaxDbContext : DbContext
{
    public CourierMaxDbContext(DbContextOptions<CourierMaxDbContext> options) : base(options) { }

    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<ShipmentStatusHistory> ShipmentStatusHistories => Set<ShipmentStatusHistory>();
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CourierMaxDbContext).Assembly);
    }
}
