namespace FlashSales.Infrastructure.Authorization
{
    public sealed class UserRole
    {
        public string IdentityProviderId { get; set; } = null!;
        public Guid UserId { get; set; }
        public string RoleName { get; set; } = null!;
    }
}