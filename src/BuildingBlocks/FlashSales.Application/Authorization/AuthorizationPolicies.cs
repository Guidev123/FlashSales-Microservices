namespace FlashSales.Application.Authorization
{
    public static class AuthorizationPolicies
    {
        public const string ScopePolicyPrefix = "scope:";

        public static string ForScopes(params string[] scopes) => ScopePolicyPrefix + string.Join(',', scopes);
    }
}
