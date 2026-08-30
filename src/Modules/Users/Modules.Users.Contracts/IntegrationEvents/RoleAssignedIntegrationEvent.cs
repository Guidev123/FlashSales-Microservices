using FlashSales.Application.Messaging;

namespace Modules.Users.Contracts.IntegrationEvents
{
    public sealed record RoleAssignedIntegrationEvent : IntegrationEvent
    {
        public static RoleAssignedIntegrationEvent Create(
            Guid correlationId,
            Guid userId,
            string identityProviderId,
            string roleName
            )
        {
            return new(correlationId, userId, identityProviderId, roleName);
        }

        private RoleAssignedIntegrationEvent(
            Guid correlationId,
            Guid userId,
            string identityProviderId,
            string roleName
            )
            : base(correlationId, nameof(RoleAssignedIntegrationEvent))
        {
            UserId = userId;
            IdentityProviderId = identityProviderId;
            RoleName = roleName;
        }

        private RoleAssignedIntegrationEvent()
        { }

        public Guid UserId { get; set; }
        public string IdentityProviderId { get; set; } = null!;
        public string RoleName { get; set; } = null!;
    }
}
