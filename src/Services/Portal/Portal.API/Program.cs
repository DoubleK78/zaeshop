using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Portal.API.Controllers;
using Portal.API.Extensions;
using Portal.API.Middlewares;
using Portal.Infrastructure;

int workerThreads, completionPortThreads;
ThreadPool.GetMinThreads(out workerThreads, out completionPortThreads);

// Set the minimum number of threads to 8 (2 vCPU * 4 threads)
// Update 6/27/2024: x3 threads
ThreadPool.SetMinThreads(24, completionPortThreads);

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = long.MaxValue;
});

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddPortalServices(builder.Configuration);
builder.Services.AddHangFireServices(builder.Configuration);
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = _ => new ValidateModelActionResult();
});

builder.Services.AddBusinessServices();
builder.Services.AddSwaggerServices();
builder.Services.AddCors();
builder.Services.AddGrpc().AddJsonTranscoding();
builder.Services.AddGrpcReflection();
builder.Services.AddResponseCompression(options =>
{
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() && (Environment.GetEnvironmentVariable("ASPNETCORE_SWAGGER_HIDE") ?? "false") == "false")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
var portalContext = services.GetRequiredService<ApplicationDbContext>();
var logger = services.GetRequiredService<ILogger<Program>>();

try
{
    if (app.Environment.IsProduction())
    {
        await portalContext.Database.MigrateAsync();
    }
}
catch (Exception ex)
{
    logger.LogError(ex, "An error occured during migration");
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
app.UseSession();

app.MapControllers();

// gRPC
app.MapGrpcService<UserGrpcController>();
app.MapGrpcReflectionService();

app.Run();
