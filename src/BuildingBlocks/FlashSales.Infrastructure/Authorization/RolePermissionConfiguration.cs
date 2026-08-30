using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlashSales.Infrastructure.Authorization
{
    public sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
    {
        public void Configure(EntityTypeBuilder<RolePermission> builder)
        {
            builder.ToTable("RolePermissions");

            builder.HasKey(x => new { x.RoleName, x.PermissionCode });

            builder.Property(x => x.RoleName).HasColumnType("VARCHAR(50)").IsRequired();
            builder.Property(x => x.PermissionCode).HasColumnType("VARCHAR(100)").IsRequired();
        }
    }
}
