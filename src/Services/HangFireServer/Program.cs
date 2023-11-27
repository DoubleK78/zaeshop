using Hangfire;
using HangFireServer.Extensions;
using HangFireServer.Filters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHangFireServices(builder.Configuration);

// Hangfire will DI of Portal to background jobs
builder.Services.AddPortalServices(builder.Configuration);
builder.Services.AddBusinessServices();
builder.Services.AddElasticsearch(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.UseAuthorization();
app.UseHangfireDashboard(options: new DashboardOptions {
    Authorization = new[] { new DashboardNoAuthorizationFilter() }
});

app.MapControllers();
app.MapHangfireDashboard();

app.Run();
