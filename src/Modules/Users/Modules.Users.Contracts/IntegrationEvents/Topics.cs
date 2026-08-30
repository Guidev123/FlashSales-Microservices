namespace Modules.Users.Contracts.IntegrationEvents
{
    public static class Topics
    {
        public const string SellerActivated = "flash-sales.users.seller-activated";
        public const string UserCreated = "flash-sales.users.user-created";
        public const string SellerProfilePictureUpdated = "flash-sales.users.seller-profile-picture-updated";
        public const string UserProfileUpdated = "flash-sales.users.user-profile-updated";
        public const string RoleAssigned = "flash-sales.users.role-assigned";
        public const string RoleUnassigned = "flash-sales.users.role-unassigned";
        public const string RolePermissionGranted = "flash-sales.users.role-permission-granted";
        public const string RolePermissionRevoked = "flash-sales.users.role-permission-revoked";
    }
}
