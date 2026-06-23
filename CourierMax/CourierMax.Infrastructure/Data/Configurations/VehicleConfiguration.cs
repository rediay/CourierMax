using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CourierMax.Domain.Entities;

namespace CourierMax.Infrastructure.Data.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("Vehicles");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Plate).HasMaxLength(20).IsRequired();
        builder.HasIndex(v => v.Plate).IsUnique();

        builder.Property(v => v.DriverId);
        builder.HasIndex(v => v.DriverId).IsUnique();

        builder.Property(v => v.MaxWeightKg).HasColumnType("decimal(10,2)").IsRequired();
        builder.Property(v => v.MaxVolumeM3).HasColumnType("decimal(10,2)").IsRequired();
        builder.Property(v => v.CurrentWeightKg).HasColumnType("decimal(10,2)").IsRequired();
        builder.Property(v => v.CurrentVolumeM3).HasColumnType("decimal(10,2)").IsRequired();

        builder.Property(v => v.CreatedAt).IsRequired();
        builder.Property(v => v.UpdatedAt);

        builder.HasOne(v => v.Driver)
            .WithOne(d => d.Vehicle)
            .HasForeignKey<Vehicle>(v => v.DriverId)
            .OnDelete(DeleteBehavior.SetNull);

        var seedDate = new DateTime(2026, 6, 10, 0, 0, 0, DateTimeKind.Utc);

        builder.HasData(
            new { Id = 1, Plate = "ABC-123", DriverId = (int?)1, MaxWeightKg = 500m, MaxVolumeM3 = 10m, CurrentWeightKg = 0m, CurrentVolumeM3 = 0m, CreatedAt = seedDate, UpdatedAt = (DateTime?)null },
            new { Id = 2, Plate = "DEF-456", DriverId = (int?)2, MaxWeightKg = 300m, MaxVolumeM3 = 6m, CurrentWeightKg = 0m, CurrentVolumeM3 = 0m, CreatedAt = seedDate, UpdatedAt = (DateTime?)null },
            new { Id = 3, Plate = "GHI-789", DriverId = (int?)3, MaxWeightKg = 800m, MaxVolumeM3 = 15m, CurrentWeightKg = 0m, CurrentVolumeM3 = 0m, CreatedAt = seedDate, UpdatedAt = (DateTime?)null }
        );
    }
}
