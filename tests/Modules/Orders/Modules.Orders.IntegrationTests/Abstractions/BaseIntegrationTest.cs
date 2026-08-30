using Bogus;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using MidR.Interfaces;
using Modules.Orders.Domain.Orders.Entities;
using Modules.Orders.Infrastructure.Database;

namespace Modules.Orders.IntegrationTests.Abstractions
{
    [Collection(nameof(IntegrationTestCollection))]
    public abstract class BaseIntegrationTest : IDisposable
    {
        private readonly IServiceScope _serviceScope;
        protected readonly IMediator _mediator;
        protected static readonly Faker _faker = new();
        protected readonly IntegrationWebApplicationFactory _factory;
        internal readonly OrdersDbContext _dbContext;
        internal readonly IDocumentStore _documentStore;

        protected BaseIntegrationTest(IntegrationWebApplicationFactory factory)
        {
            _factory = factory;
            _serviceScope = factory.Services.CreateScope();
            _mediator = _serviceScope.ServiceProvider.GetRequiredService<IMediator>();
            _dbContext = _serviceScope.ServiceProvider.GetRequiredService<OrdersDbContext>();
            _documentStore = _serviceScope.ServiceProvider.GetRequiredService<IDocumentStore>();
        }

        protected async Task<TResponse> SendInNewScopeAsync<TResponse>(IRequest<TResponse> request)
        {
            await using var scope = _factory.Services.CreateAsyncScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            return await mediator.SendAsync(request);
        }

        protected async Task<Order?> GetOrderAsync(Guid orderId)
        {
            await using var session = _documentStore.QuerySession();
            return await session.LoadAsync<Order>(orderId);
        }

        protected async Task<Order?> GetOrderByCustomerAndLaunchAsync(Guid customerId, Guid launchId)
        {
            await using var session = _documentStore.QuerySession();
            return await session.Query<Order>().FirstOrDefaultAsync(o => o.CustomerId == customerId && o.LaunchId == launchId);
        }

        protected async Task<int> CountOrdersByCustomerAndLaunchAsync(Guid customerId, Guid launchId)
        {
            await using var session = _documentStore.QuerySession();
            return await session.Query<Order>().CountAsync(o => o.CustomerId == customerId && o.LaunchId == launchId);
        }

        public void Dispose()
        {
            _serviceScope.Dispose();
        }
    }
}
