using I_am_engineer.Identity.Application.DependencyInjection;
using I_am_engineer.Identity.Infrastructure.DependencyInjection;
using I_am_engineer.Identity.Transport.Grpc.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddGrpc();
builder.Services.AddHttpContextAccessor();
builder.Services
    .AddIdentityApplication()
    .AddIdentityInfrastructure();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Identity API v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapGrpcService<IdentityGrpcService>();
app.MapControllers();

app.Run();

public partial class Program;
