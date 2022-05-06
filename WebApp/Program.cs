using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using WebApp.Authorizations;
using WebApp.Configuration;
using WebApp.Data;
using WebApp.Extensions;
using WebApp.Helpers;
using WebApp.Models;
using WebApp.Services;

IConfiguration BuildLoggerConfiguration(string[] args)
{
    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

    var configurationBuilder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("serilog.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"serilog.{environment}.json", optional: true, reloadOnChange: true);

    configurationBuilder.AddCommandLine(args);
    configurationBuilder.AddEnvironmentVariables();

    return configurationBuilder.Build();
}

void ConfigureConfiguration(IWebHostEnvironment environment, IConfigurationBuilder configuration)
{
    configuration
        .AddJsonFile("serilog.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"serilog.{environment.EnvironmentName}.json", optional: true, reloadOnChange: true);
}

void ConfigureServices(IWebHostEnvironment environment, IConfiguration configuration, IServiceCollection services)
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

    ConfigureConfiguration(environment, configuration);
    ConfigureServices(environment, configuration, services);

    builder.Host.UseSerilog((ctx, sp, loggerConfig) =>
    {
        loggerConfig
            .ReadFrom.Configuration(ctx.Configuration)
            .ReadFrom.Services(sp)
            .Enrich.FromLogContext();
    });

    var app = builder.Build();

    ConfigureApplication(app);

    return app;
}

var loggerConfiguration = BuildLoggerConfiguration(args);
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(loggerConfiguration)
    .CreateLogger();
try
{
    var app = CreateApplication();
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}