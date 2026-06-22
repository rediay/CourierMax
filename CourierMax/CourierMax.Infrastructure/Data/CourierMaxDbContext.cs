using Microsoft.EntityFrameworkCore;
using CourierMax.Domain.Entities;
using CourierMax.Domain.Enums;
using CourierMax.Domain.ValueObjects;

namespace CourierMax.Infrastructure.Data;

public class CourierMaxDbContext : DbContext
{
    public CourierMaxDbContext(DbContextOptions<CourierMaxDbContext> options) : base(options) { }

    public DbSet<Shipment> Shipments => Set<Shipment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CourierMaxDbContext).Assembly);
    }
}
