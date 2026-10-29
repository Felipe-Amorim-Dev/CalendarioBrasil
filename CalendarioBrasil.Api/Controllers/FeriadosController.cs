using CalendarioBrasil.Application.DTOs;
using CalendarioBrasil.Application.Interfaces;
using CalendarioBrasil.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CalendarioBrasil.Api.Controllers
{
    [ApiController]
    [Route("api/v1/feriados")]
    [Produces("application/json")]
    public sealed class FeriadosController : ControllerBase
    {
        private readonly IFeriadosNacionaisFixosService _service;

        public FeriadosController(IFeriadosNacionaisFixosService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        [HttpGet("nacionais/fixos")]
        [ProducesResponseType(typeof(IReadOnlyList<FeriadoNacionalFixoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public ActionResult<IReadOnlyList<FeriadoNacionalFixoDto>> ObterNacionaisFixos([FromQuery, Range(AnoCalendario.AnoMinimo, AnoCalendario.AnoMaximo)] int ano)
        {
            var feriados = _service.ObterPorAno(ano);

            return Ok(feriados);
        }
    }
}
