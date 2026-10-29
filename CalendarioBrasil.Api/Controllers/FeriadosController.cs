using CalendarioBrasil.Application.DTOs;
using CalendarioBrasil.Application.Interfaces;
using CalendarioBrasil.Application.Constants;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CalendarioBrasil.Api.Controllers
{
    [ApiController]
    [Route("api/v1/feriados")]
    [Produces("application/json")]
    public sealed class FeriadosController : ControllerBase
    {
        private readonly IFeriadosNacionaisFixosService _fixosService;
        private readonly ICalendarioService _calendarioService;

        public FeriadosController(IFeriadosNacionaisFixosService fixosService, ICalendarioService calendarioService)
        {
            _fixosService = fixosService ?? throw new ArgumentNullException(nameof(fixosService));
            _calendarioService = calendarioService ?? throw new ArgumentNullException(nameof(calendarioService));
        }

        [HttpGet("nacionais/fixos")]
        [ProducesResponseType(typeof(IReadOnlyList<FeriadoNacionalFixoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public ActionResult<IReadOnlyList<FeriadoNacionalFixoDto>> ObterNacionaisFixos([FromQuery, Range(LimitesConsultaCalendario.AnoMinimo, LimitesConsultaCalendario.AnoMaximo)] int ano)
        {
            return Ok(_fixosService.ObterPorAno(ano));
        }

        [HttpGet]
        [ProducesResponseType(typeof(CalendarioFeriadosDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public ActionResult<CalendarioFeriadosDto> Obter([FromQuery, Range(LimitesConsultaCalendario.AnoMinimo, LimitesConsultaCalendario.AnoMaximo)] int ano, [FromQuery, Required, StringLength(2, MinimumLength = 2)] string? uf)
        {
            if (string.IsNullOrWhiteSpace(uf))
            {
                ModelState.AddModelError(nameof(uf), "A UF é obrigatória.");
                return ValidationProblem(ModelState);
            }

            if (!_calendarioService.SuportaUf(uf))
            {
                ModelState.AddModelError(nameof(uf), $"UF ainda não suportada. Disponíveis: {string.Join(", ", _calendarioService.ObterUfsSuportadas())}.");
                return ValidationProblem(ModelState);
            }

            return Ok(_calendarioService.ObterPorAnoEUf(ano, uf));
        }
    }
}
