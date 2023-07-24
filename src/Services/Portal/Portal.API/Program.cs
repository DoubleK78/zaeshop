using Portal.API.Controllers;
using Portal.API.Extensions;
using Portal.API.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddPortalServices(builder.Configuration);
builder.Services.AddHangFireServices(builder.Configuration);
builder.Services.AddSwaggerServices();
builder.Services.AddCors();
builder.Services.AddGrpc().AddJsonTranscoding();
builder.Services.AddGrpcReflection();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(x => x
   .SetIsOriginAllowed(origin => true)
   .AllowAnyMethod()
   .AllowAnyHeader()
   .AllowCredentials());

app.UseMiddleware<JwtMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseAuthorization();

app.MapControllers();

// gRPC
app.MapGrpcService<UserGrpcController>();
app.MapGrpcReflectionService();

app.Run();
