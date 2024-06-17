using Hangfire;
using HangFireServer.Extensions;
using HangFireServer.Filters;
using HangFireServer.HealthCheck;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHangFireServices(builder.Configuration);
builder.Services.AddHealthChecks().AddCheck<HangfireHealthCheck>("hangfire");

// Hangfire will DI of Portal to background jobs
builder.Services.AddPortalServices(builder.Configuration);
builder.Services.AddBusinessServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.UseAuthorization();
app.UseHangfireDashboard(options: new DashboardOptions
{
    Authorization = new[] { new DashboardNoAuthorizationFilter() }
});

app.MapHealthChecks("/healthz");
app.MapControllers();
app.MapHangfireDashboard();

app.StartHangFireJobs();
app.Run();
