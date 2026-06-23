using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CourierMax.Domain.Entities;
using CourierMax.Domain.Enums;

namespace CourierMax.Infrastructure.Data.Configurations;

public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.ToTable("Shipments");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.TrackingCode)
            .HasConversion(
                tc => tc.Value,
                value => Domain.ValueObjects.TrackingCode.FromString(value))
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(s => s.TrackingCode).IsUnique();

        builder.Property(s => s.SenderName).HasMaxLength(100).IsRequired();
        builder.Property(s => s.SenderPhone)
            .HasConversion(
                p => p.Value,
                value => new Domain.ValueObjects.Phone(value))
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(s => s.SenderAddress)
            .HasConversion(
                a => a.Value,
                value => new Domain.ValueObjects.Address(value))
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.RecipientName).HasMaxLength(100).IsRequired();
        builder.Property(s => s.RecipientPhone)
            .HasConversion(
                p => p.Value,
                value => new Domain.ValueObjects.Phone(value))
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(s => s.RecipientAddress)
            .HasConversion(
                a => a.Value,
                value => new Domain.ValueObjects.Address(value))
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.PackageWeight)
            .HasConversion(
                w => w.Kg,
                value => new Domain.ValueObjects.Weight(value))
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.OwnsOne(s => s.PackageDimensions, dim =>
        {
            dim.Property(d => d.LengthCm).HasColumnName("PackageLength").HasColumnType("decimal(10,2)").IsRequired();
            dim.Property(d => d.WidthCm).HasColumnName("PackageWidth").HasColumnType("decimal(10,2)").IsRequired();
            dim.Property(d => d.HeightCm).HasColumnName("PackageHeight").HasColumnType("decimal(10,2)").IsRequired();
        });

        builder.Property(s => s.PackageType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(s => s.ServiceType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(s => s.Origin).HasMaxLength(50).IsRequired();
        builder.Property(s => s.Destination).HasMaxLength(50).IsRequired();

        builder.Property(s => s.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(s => s.VehicleId);
        builder.Property(s => s.DriverId);

        builder.Property(s => s.TotalCost)
            .HasColumnType("decimal(18,2)");

        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.UpdatedAt);

        builder.Metadata.FindNavigation(nameof(Shipment.StatusHistories))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
