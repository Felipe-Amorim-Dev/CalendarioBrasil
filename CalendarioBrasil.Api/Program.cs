using CalendarioBrasil.Application.Interfaces;
using CalendarioBrasil.Application.Services;
using CalendarioBrasil.Domain.Services;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();

builder.Services.AddSingleton<GeradorFeriadosNacionaisFixos>();
builder.Services.AddScoped<IFeriadosNacionaisFixosService, FeriadosNacionaisFixosService>();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("api", limiter =>
    {
        limiter.PermitLimit = 60;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
        limiter.AutoReplenishment = true;
    });
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseRouting();
app.UseRateLimiter();

app.MapControllers().RequireRateLimiting("api");

app.Run();