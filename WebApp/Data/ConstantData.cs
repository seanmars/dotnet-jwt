namespace WebApp.Data;

public static class ConstantData
{
    public struct DefaultUserData
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public List<string> Roles { get; set; }
    }


    public static class DefaultRole
    {
        public static readonly string SuperAdminRole = "SuperAdmin".ToLower();
        public static readonly string AdminRole = "Admin".ToLower();
        public static readonly string Member = "Member".ToLower();

        public static readonly List<string> Roles = new(2)
        {
            SuperAdminRole,
            AdminRole,
            Member
        };
    }

    public static class DefaultUser
    {
        public static readonly DefaultUserData SuperAdmin = new()
        {
            Id = 1,
            Email = "superadmin@app.com",
            Password = "superadmin0000",
            Roles = new List<string>(1)
            {
                DefaultRole.SuperAdminRole
            }
        };

        public static readonly DefaultUserData Admin = new()
        {
            Id = 2,
            Email = "admin@app.com",
            Password = "admin0000",
            Roles = new List<string>(1)
            {
                DefaultRole.AdminRole
            }
        };

        public static readonly List<DefaultUserData> Users = new(2)
        {
            SuperAdmin,
            Admin
        };
    }
}