using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlashSales.Infrastructure.Authorization
{
    public sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.ToTable("UserRoles");

            builder.HasKey(x => new { x.IdentityProviderId, x.RoleName });

            builder.Property(x => x.IdentityProviderId).HasColumnType("VARCHAR(100)").IsRequired();
            builder.Property(x => x.UserId).IsRequired();
            builder.Property(x => x.RoleName).HasColumnType("VARCHAR(50)").IsRequired();
        }
    }
}
