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
    }
}
