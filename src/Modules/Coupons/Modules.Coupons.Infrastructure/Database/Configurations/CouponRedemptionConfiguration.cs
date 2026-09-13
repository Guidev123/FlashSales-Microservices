using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Coupons.Domain.Coupons.Entities;

namespace Modules.Coupons.Infrastructure.Database.Configurations
{
    internal sealed class CouponRedemptionConfiguration : IEntityTypeConfiguration<CouponRedemption>
    {
        public void Configure(EntityTypeBuilder<CouponRedemption> builder)
        {
            builder.ToTable("CouponRedemptions");

            builder.HasKey(cr => cr.Id);

            builder.Property(cr => cr.CouponId)
                .IsRequired();

            builder.Property(cr => cr.OrderId)
                .IsRequired();

            builder.Property(cr => cr.CustomerId)
                .IsRequired();

            builder.Property(cr => cr.CreatedOn)
                .IsRequired();
        }
    }
}