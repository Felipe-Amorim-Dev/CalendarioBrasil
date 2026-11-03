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
        public ActionResult<CalendarioFeriadosDto> Obter([FromQuery, Range(LimitesConsultaCalendario.AnoMinimo, LimitesConsultaCalendario.AnoMaximo)] int ano, [FromQuery(Name = "uf")] string[]? ufs = null, [FromQuery] string escopo = "Geral")
        {
            if (!ConversorEscopoCalendario.TentarConverter(escopo, out _))
            {
                ModelState.AddModelError(nameof(escopo), "Escopo inválido. Utilize Geral, ExecutivoEstadual ou JudiciarioEstadual.");
                return ValidationProblem(ModelState);
            }

            var solicitadas = (ufs ?? Array.Empty<string>())
                .SelectMany(valor => (valor ?? string.Empty).Split(',', StringSplitOptions.None))
                .Select(valor => valor.Trim().ToUpperInvariant())
                .ToArray();

            if (solicitadas.Any(uf => uf.Length != 2 || uf.Any(caractere => caractere is < 'A' or > 'Z') || !_calendarioService.SuportaUf(uf)))
            {
                ModelState.AddModelError("uf", $"Uma ou mais UFs são inválidas. Disponíveis: {string.Join(", ", _calendarioService.ObterUfsSuportadas())}.");
                return ValidationProblem(ModelState);
            }

            return Ok(_calendarioService.ObterPorAno(ano, solicitadas, escopo));
        }
    }
}
