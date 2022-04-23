using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebApp.Configuration;
using WebApp.Data;
using WebApp.Extensions;
using WebApp.Models;
using WebApp.Services;

void SettingService(IConfiguration configuration, IServiceCollection services)
{
    services.AddCors();
    services.AddControllers();

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();

    services.Configure<JwtOption>(configuration.GetSection("Jwt"));

    services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(
            configuration.GetConnectionString("DefaultConnection")));

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
                ValidIssuer = configuration[JwtService.JwtIssueKey],
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = false,
                IssuerSigningKey = new SymmetricSecurityKey(JwtService.GetKeyBytes(configuration[JwtService.JwtSecretKey]))
            };
        });

    services.AddAuthorization(options =>
    {
        // Setting the default authorization policy for JwtBearer
        options.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
            .RequireAuthenticatedUser()
            .Build();
    });

    services.AddSingleton<JwtService>();
    services.AddScoped<SignInManager>();
    services.AddScoped<AccountService>();
}

void ConfigureApplication(WebApplication app)
{
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
}

WebApplication CreateApplication()
{
    var builder = WebApplication.CreateBuilder(args);
    var configuration = builder.Configuration;
    var services = builder.Services;

    SettingService(configuration, services);

    var app = builder.Build();
    ConfigureApplication(app);

    return app;
}

var app = CreateApplication();
app.Run();