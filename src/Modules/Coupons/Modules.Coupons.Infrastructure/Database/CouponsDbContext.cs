using FlashSales.Domain.DomainObjects;
using FlashSales.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Modules.Coupons.Domain.Coupons.Entities;
using System.Reflection;

namespace Modules.Coupons.Infrastructure.Database
{
    internal sealed class CouponsDbContext(DbContextOptions<CouponsDbContext> options) : DbContext(options)
    {
        public DbSet<Coupon> Coupons { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(Schemas.Coupons);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            modelBuilder.Ignore<DomainEvent>();

            modelBuilder.AddOutboxAndInbox();
            modelBuilder.AddPermissions();

            base.OnModelCreating(modelBuilder);
        }
    }
}