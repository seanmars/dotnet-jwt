using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebApp.Configuration;
using WebApp.Data;
using WebApp.Models;
using WebApp.Services;

WebApplication CreateApplication()
{
    var builder = WebApplication.CreateBuilder(args);
    var configuration = builder.Configuration;
    var services = builder.Services;

    #region Setting Services

    services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();

    services.Configure<JwtOption>(builder.Configuration.GetSection("Jwt"));
    services.AddOptions<JwtOption>();

    services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(
            configuration.GetConnectionString("DefaultConnection")));

    services.AddIdentity<ApplicationUser, ApplicationRole>(
            options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                options.User.RequireUniqueEmail = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
            })
        .AddRoles<ApplicationRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

    services.AddCors();

    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.IncludeErrorDetails = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
                RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
                ValidateIssuer = true,
                ValidIssuer = configuration[JwtService.JwtIssueKey],
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = false,
                IssuerSigningKey = new SymmetricSecurityKey(JwtService.GetKeyBytes(configuration[JwtService.JwtSecretKey]))
            };
        });

    services.ConfigureApplicationCookie(options =>
    {
        options.AccessDeniedPath = "/access-denied";
        options.Cookie.Name = "backend-app";
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
        options.SlidingExpiration = true;
    });

    services.AddAuthorization(options =>
    {
        options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
    });

    services.AddSingleton<JwtService>();
    services.AddScoped<AccountService>();

    #endregion

    #region Settings Application

    var app = builder.Build();
    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseDeveloperExceptionPage();
    }

    app.UseHttpsRedirection();

    app.UseCors(corsPolicyBuilder =>
    {
        corsPolicyBuilder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    #endregion

    return app;
}

var app = CreateApplication();
app.Run();