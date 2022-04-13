namespace WebApp.Identity;

public struct PermissionInfo
{
    public string Name { get; set; }
    public string Description { get; set; }
}

public static class RolePermission
{
    public const string ClaimName = "permission";

    public static class ClaimValue
    {
        public const string RoleClaim = "role-claim";
        public const string GameData = "game-data";
    }

    public static readonly List<PermissionInfo> AllPermission = new()
    {
        new PermissionInfo()
        {
            Name = ClaimValue.RoleClaim,
            Description = "Access to the role claim page"
        },
        new PermissionInfo()
        {
            Name = ClaimValue.GameData,
            Description = "Access to the game data page"
        }
    };
}