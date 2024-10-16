using Identity.API.Controllers;
using Identity.API.Extensions;
using Identity.API.HealthCheck;
using Identity.API.Middlewares;
using Identity.Domain.AggregatesModel.UserAggregate;
using Identity.Domain.Models.Helpers;
using Identity.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddHealthChecks().AddCheck<SampleHealthCheck>("sample");

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerServices();
builder.Services.AddIdentityServices(builder.Configuration);

builder.Services.AddCors();
builder.Services.AddGrpc().AddJsonTranscoding();
builder.Services.AddGrpcReflection();

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = _ => new ValidateModelActionResult();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() && (Environment.GetEnvironmentVariable("ASPNETCORE_SWAGGER_HIDE") ?? "false") == "false")
{
    app.UseSwaggerDocumentation();
}

app.UseHttpsRedirection();

app.UseCors(x => x
    .SetIsOriginAllowed(origin => origin.Contains("localhost") || origin.Contains("127.0.0.1") || origin.EndsWith(".github.io") || origin.EndsWith(".codegota.me") || origin.Contains("fastscans.net") || origin.Contains("fastscansz.com") || origin.Contains("fastscans1.net") || origin.Contains("fastscans2.net"))
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials());

app.UseMiddleware<JwtMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseAuthorization();

app.MapHealthChecks("/healthz");
app.MapControllers();

// gRPC
app.MapGrpcService<UserGrpcController>();
app.MapGrpcReflectionService();

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
var identityContext = services.GetRequiredService<AppIdentityDbContext>();
var userManager = services.GetRequiredService<UserManager<User>>();
var logger = services.GetRequiredService<ILogger<Program>>();
try
{
    if (app.Environment.IsProduction())
    {
        await identityContext.Database.MigrateAsync();
    }
    await AppIdentityDbContextSeed.SeedUsersAsync(userManager);
}
catch (Exception ex)
{
    logger.LogError(ex, "An error occured during migration");
}

app.Run();
