using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CourierMax.Domain.Entities;
using CourierMax.Domain.Enums;

namespace CourierMax.Infrastructure.Data.Configurations;

public class ShipmentStatusHistoryConfiguration : IEntityTypeConfiguration<ShipmentStatusHistory>
{
    public void Configure(EntityTypeBuilder<ShipmentStatusHistory> builder)
    {
        builder.ToTable("ShipmentStatusHistories");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.PreviousStatus)
            .HasConversion<int>();

        builder.Property(h => h.NewStatus)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(h => h.ChangedAt)
            .IsRequired();

        builder.Property(h => h.Reason)
            .HasMaxLength(500);

        builder.Property(h => h.ChangedBy)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasOne<Shipment>()
            .WithMany(s => s.StatusHistories)
            .HasForeignKey(h => h.ShipmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
