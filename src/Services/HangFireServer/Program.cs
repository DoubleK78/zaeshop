using Hangfire;
using Hangfire.Dashboard.BasicAuthorization;
using HangFireServer.Extensions;
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
    Authorization = new[]
    {
        new BasicAuthAuthorizationFilter(new BasicAuthAuthorizationFilterOptions
        {
            RequireSsl = false,
            SslRedirect = false,
            LoginCaseSensitive = true,
            Users = new[]
            {
                new BasicAuthAuthorizationUser
                {
                    Login = "hangfire",
                    Password = new byte[] { 0x2e,0x31,0x9a,0xee,0x2e,0xf7,0x63,0x67,0xf1,0x42,0x0b,0x75,0x1a,0xce,0x38,0x27,0x12,0x15,0x67,0x48 }
                }
            }
        })
    }
});

app.MapHealthChecks("/healthz");
app.MapControllers();
app.MapHangfireDashboard();

app.StartHangFireJobs();
app.Run();
