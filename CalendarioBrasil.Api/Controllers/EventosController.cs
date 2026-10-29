using CalendarioBrasil.Application.Constants;
using CalendarioBrasil.Application.DTOs;
using CalendarioBrasil.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CalendarioBrasil.Api.Controllers
{
    [ApiController]
    [Route("api/v1/eventos")]
    [Produces("application/json")]
    public sealed class EventosController : ControllerBase
    {
        private readonly IEventosService _service;

        public EventosController(IEventosService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        [HttpGet]
        [ProducesResponseType(typeof(CalendarioEventosDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public ActionResult<CalendarioEventosDto> Obter([FromQuery, Range(LimitesConsultaCalendario.AnoMinimo, LimitesConsultaCalendario.AnoMaximo)] int ano, [FromQuery, Required, StringLength(2, MinimumLength = 2)] string? uf, [FromQuery] bool incluirPontosFacultativosFederais = false)
        {
            if (!_service.SuportaUf(uf))
            {
                ModelState.AddModelError(nameof(uf), $"UF ainda não suportada. Disponíveis: {string.Join(", ", _service.ObterUfsSuportadas())}.");
                return ValidationProblem(ModelState);
            }

            return Ok(_service.ObterPorAnoEUf(ano, uf!, incluirPontosFacultativosFederais));
        }
    }
}
