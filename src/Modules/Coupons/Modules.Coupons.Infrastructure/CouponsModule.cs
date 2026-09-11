using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Modules.Coupons.Infrastructure
{
    public static class CouponsModule
    {
        public static readonly Assembly[] Assemblies =
        [
            Application.AssemblyReference.Assembly,
            Domain.AssemblyReference.Assembly,
            Assembly.GetExecutingAssembly()
        ];

        public static IServiceCollection AddCouponsModule(this IServiceCollection services, IConfiguration configuration)
        {
            return services;
        }
    }
}