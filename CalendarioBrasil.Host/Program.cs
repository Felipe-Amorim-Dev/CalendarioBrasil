using CalendarioBrasil.Api.Controllers;
using CalendarioBrasil.Application.Interfaces;
using CalendarioBrasil.Application.Services;
using CalendarioBrasil.Domain.Interfaces;
using CalendarioBrasil.Domain.Services;
using CalendarioBrasil.Infrastructure.Calendarios;
using CalendarioBrasil.Infrastructure.Catalogos;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddApplicationPart(typeof(FeriadosController).Assembly);
builder.Services.AddProblemDetails();

builder.Services.AddSingleton<GeradorFeriadosNacionaisFixos>();
builder.Services.AddSingleton<CalculadoraDatasMoveis>();
builder.Services.AddSingleton<CalculadoraDatasRecorrentes>();

builder.Services.AddSingleton<IGeradorFeriadosEstaduais, GeradorFeriadosEstaduais>();
builder.Services.AddSingleton<ICatalogoEventosComplementares, CatalogoEventosComplementares>();
builder.Services.AddScoped<ICoberturaEstadualService, CoberturaEstadualService>();

builder.Services.AddScoped<IFeriadosNacionaisFixosService, FeriadosNacionaisFixosService>();
builder.Services.AddScoped<ICalendarioService, CalendarioService>();
builder.Services.AddScoped<IDatasMoveisService, DatasMoveisService>();
builder.Services.AddScoped<IEventosService, EventosService>();

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