namespace Modules.Orders.Domain.Orders.Repositories
{
    public sealed class ActiveOrderAlreadyExistsException(Guid launchId) : Exception
    {
        public Guid LaunchId { get; } = launchId;
    }
}
