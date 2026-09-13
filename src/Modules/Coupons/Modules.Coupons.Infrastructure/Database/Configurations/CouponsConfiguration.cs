using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Coupons.Domain.Coupons.Entities;

namespace Modules.Coupons.Infrastructure.Database.Configurations
{
    internal sealed class CouponsConfiguration : IEntityTypeConfiguration<Coupon>
    {
        public void Configure(EntityTypeBuilder<Coupon> builder)
        {
            builder.ToTable("Coupons");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.LaunchId)
                .IsRequired();

            builder.Property(c => c.SellerId)
                .IsRequired();

            builder.Property(c => c.MinimumOrderAmount)
                .IsRequired(false);

            builder.Property(c => c.MaxRedemptionsPerCustomer)
                .IsRequired(false);

            builder.Property(c => c.Status)
                .HasConversion<string>()
                .HasColumnType("VARCHAR(20)")
                .IsRequired();

            builder.Property(c => c.Code)
                .HasColumnType("VARCHAR(50)")
                .IsRequired();

            builder.OwnsOne(c => c.Discount, discount =>
            {
                discount.Property(d => d.Type)
                    .HasConversion<string>()
                    .HasColumnType("VARCHAR(20)")
                    .HasColumnName("Type")
                    .IsRequired();

                discount.Property(d => d.MaxDiscountAmount)
                    .HasColumnType("DECIMAL(18,2)")
                    .HasColumnName("MaxDiscountAmount")
                    .IsRequired(false);

                discount.Property(d => d.Value)
                    .HasColumnType("DECIMAL(18,2)")
                    .HasColumnName("Value")
                    .IsRequired();
            });

            builder.OwnsOne(c => c.Validity, validity =>
            {
                validity.Property(vp => vp.ValidUntil)
                    .HasColumnName("ValidUntil")
                    .IsRequired();

                validity.Property(vp => vp.ValidFrom)
                    .HasColumnName("ValidFrom")
                    .IsRequired();
            });

            builder.OwnsOne(c => c.Usage, usage =>
            {
                usage.Property(u => u.MaxRedemptions)
                    .HasColumnName("MaxRedemptions")
                    .IsRequired();

                usage.Property(u => u.RedeemedCount)
                    .HasColumnName("RedeemedCount")
                    .IsRequired();
            });

            builder.Property(l => l.CreatedOn)
                .IsRequired();

            builder.Property<uint>("Version")
                .HasColumnName("xmin")
                .HasColumnType("xid")
                .IsRowVersion();

            builder.HasMany(c => c.Redemptions)
                .WithOne()
                .HasForeignKey(c => c.CouponId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(l => l.Redemptions).HasField("_redemptions");

            builder.HasIndex(c => c.Code)
                .IsUnique();
        }
    }
}