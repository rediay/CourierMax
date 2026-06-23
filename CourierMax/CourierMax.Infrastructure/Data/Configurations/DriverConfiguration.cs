using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CourierMax.Domain.Entities;

namespace CourierMax.Infrastructure.Data.Configurations;

public class DriverConfiguration : IEntityTypeConfiguration<Driver>
{
    public void Configure(EntityTypeBuilder<Driver> builder)
    {
        builder.ToTable("Drivers");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name).HasMaxLength(100).IsRequired();
        builder.Property(d => d.Phone).HasMaxLength(20);
        builder.Property(d => d.Email).HasMaxLength(100);
        builder.Property(d => d.IsActive).IsRequired();
        builder.Property(d => d.CreatedAt).IsRequired();
        builder.Property(d => d.UpdatedAt);
    }
}
