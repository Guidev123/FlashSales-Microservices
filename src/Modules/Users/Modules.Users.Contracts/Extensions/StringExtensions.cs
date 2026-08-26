using FlashSales.Users.Contracts.Protos;

namespace Modules.Users.Contracts.Extensions
{
    public static class StringExtensions
    {
        public static GetUserPermissionsRequest MapToPermissionRequest(this string identityId)
            => new() { IdentityId = identityId };
    }
}