using FlashSales.Application.Messaging;

namespace Modules.Users.Contracts.IntegrationEvents
{
    public sealed record RoleUnassignedIntegrationEvent : IntegrationEvent
    {
        public static RoleUnassignedIntegrationEvent Create(
            Guid correlationId,
            string identityProviderId,
            string roleName
            )
        {
            return new(correlationId, identityProviderId, roleName);
        }

        private RoleUnassignedIntegrationEvent(
            Guid correlationId,
            string identityProviderId,
            string roleName
            )
            : base(correlationId, nameof(RoleUnassignedIntegrationEvent))
        {
            IdentityProviderId = identityProviderId;
            RoleName = roleName;
        }

        private RoleUnassignedIntegrationEvent()
        { }

        public string IdentityProviderId { get; set; } = null!;
        public string RoleName { get; set; } = null!;
    }
}
