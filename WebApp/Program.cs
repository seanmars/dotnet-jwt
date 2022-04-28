using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using WebApp.Authorizations;
using WebApp.Configuration;
using WebApp.Data;
using WebApp.Extensions;
using WebApp.Helpers;
using WebApp.Models;
using WebApp.Services;

void SettingService(IWebHostEnvironment environment, IConfiguration configuration, IServiceCollection services)
{
    if (environment == null) throw new ArgumentNullException(nameof(environment));
    if (configuration == null) throw new ArgumentNullException(nameof(configuration));
    if (services == null) throw new ArgumentNullException(nameof(services));

    services.AddCors();
    services.AddControllers();

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();

    services.Configure<JwtOption>(configuration.GetSection("Jwt"));

    services.AddDbContext<ApplicationDbContext>(options =>
    {
        if (environment.IsDevelopment())
        {
            options.EnableDetailedErrors();
            options.EnableSensitiveDataLogging();
        }

        options.UseSqlite(
            configuration.GetConnectionString("DefaultConnection"));
    });


    services.AddBaseIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.SignIn.RequireConfirmedAccount = true;
            options.User.RequireUniqueEmail = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.IncludeErrorDetails = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = configuration[JwtOption.IssueKeyName],
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = false,
                IssuerSigningKey = new SymmetricSecurityKey(JwtHelper.GetKeyBytes(configuration[JwtOption.SecretKeyName]))
            };
        });

    services.AddTransient<IAuthorizationHandler, RolePermissionAuthorizationHandler>();
    services.AddSingleton<JwtHelper>();
    services.AddScoped<AccountService>();
    services.AddScoped<SignInManager>();
}

void ConfigureApplication(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        IdentityModelEventSource.ShowPII = true;
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

    // 驗證 JWT
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
}

WebApplication CreateApplication()
{
    var builder = WebApplication.CreateBuilder(args);

    var environment = builder.Environment;
    var configuration = builder.Configuration;
    var services = builder.Services;

    SettingService(environment, configuration, services);

    var app = builder.Build();
    ConfigureApplication(app);

    return app;
}

var app = CreateApplication();
app.Run();