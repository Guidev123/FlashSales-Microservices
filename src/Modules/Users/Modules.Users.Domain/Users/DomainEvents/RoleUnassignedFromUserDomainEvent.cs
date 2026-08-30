using FlashSales.Domain.DomainObjects;

namespace Modules.Users.Domain.Users.DomainEvents
{
    public sealed record RoleUnassignedFromUserDomainEvent : DomainEvent
    {
        public static RoleUnassignedFromUserDomainEvent Create(Guid userId, string role)
            => new(userId, role);

        private RoleUnassignedFromUserDomainEvent(Guid userId, string role)
            : base(userId, nameof(RoleUnassignedFromUserDomainEvent))
        {
            UserId = userId;
            Role = role;
        }

        private RoleUnassignedFromUserDomainEvent()
        { }

        public Guid UserId { get; set; }
        public string Role { get; set; } = null!;
    }
}
