using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CourierMax.Domain.Entities;

namespace CourierMax.Infrastructure.Data.Configurations;

public class CityDistanceConfiguration : IEntityTypeConfiguration<CityDistance>
{
    public void Configure(EntityTypeBuilder<CityDistance> builder)
    {
        builder.ToTable("CityDistances");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Origin).HasMaxLength(50).IsRequired();
        builder.Property(c => c.Destination).HasMaxLength(50).IsRequired();
        builder.HasIndex(c => new { c.Origin, c.Destination }).IsUnique();

        builder.Property(c => c.DistanceKm).HasColumnType("decimal(10,2)").IsRequired();
        builder.Property(c => c.DistanceFee).HasColumnType("decimal(18,2)").IsRequired();

        builder.HasData(
            new { Id = 1, Origin = "Bogotá", Destination = "Medellín", DistanceKm = 480m, DistanceFee = 12000m },
            new { Id = 2, Origin = "Bogotá", Destination = "Cali", DistanceKm = 360m, DistanceFee = 9000m },
            new { Id = 3, Origin = "Bogotá", Destination = "Barranquilla", DistanceKm = 950m, DistanceFee = 20000m },
            new { Id = 4, Origin = "Medellín", Destination = "Cali", DistanceKm = 310m, DistanceFee = 8000m },
            new { Id = 5, Origin = "Medellín", Destination = "Barranquilla", DistanceKm = 650m, DistanceFee = 15000m },
            new { Id = 6, Origin = "Cali", Destination = "Barranquilla", DistanceKm = 900m, DistanceFee = 18000m }
        );
    }
}
