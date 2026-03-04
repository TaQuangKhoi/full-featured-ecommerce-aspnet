using ECommerce.Application;
using ECommerce.Infrastructure;
using ECommerce.Infrastructure.Data;
using Serilog;
using Microsoft.OpenApi.Models;
using System.Threading.RateLimiting;

// Bootstrap logger captures startup errors before full Serilog configuration loads
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("Logs/startup-.txt", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

Log.Information("Starting ECommerce application...");

try
{

var builder = WebApplication.CreateBuilder(args);

// Load local overrides (gitignored, use for local secrets/connection strings)
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

// Serilog - replace bootstrap logger with full configuration from appsettings
builder.Host.UseSerilog((context, services, config) =>
    config.ReadFrom.Configuration(context.Configuration)
          .ReadFrom.Services(services)
          .Enrich.WithProperty("MachineName", Environment.MachineName)
          .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName));

Log.Information("Environment: {Environment}", builder.Environment.EnvironmentName);
Log.Information("ContentRoot: {ContentRoot}", builder.Environment.ContentRootPath);
Log.Information("Connection string configured: {HasConnectionString}",
    !string.IsNullOrEmpty(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services
builder.Services.AddControllersWithViews();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ECommerce API", Version = "v1" });
});

// Rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
    options.RejectionStatusCode = 429;
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    await DbSeeder.SeedAsync(scope.ServiceProvider);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "admin",
    pattern: "Admin/{controller=Admin}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

Log.Information("Application configured successfully. Starting web server...");
app.Run();

}
catch (Exception ex) when (ex is not OperationCanceledException &&
    ex.GetType().FullName != "Microsoft.Extensions.Hosting.Internal.StopTheHostException")
{
    Log.Fatal(ex, "Application terminated unexpectedly during startup");
    throw;
}
finally
{
    Log.Information("Application shut down.");
    Log.CloseAndFlush();
}
