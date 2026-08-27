using Bogus;

namespace Modules.Orders.IntegrationTests.Abstractions.Helpers
{
    internal sealed record TestSeller(Guid UserId, Guid SellerId, string Name);

    internal static class SellerHelper
    {
        internal static TestSeller Create(Faker faker)
            => new(Guid.NewGuid(), Guid.NewGuid(), faker.Company.CompanyName());
    }
}
