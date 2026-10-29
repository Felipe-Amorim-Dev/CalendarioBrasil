using CalendarioBrasil.Application.Constants;
using CalendarioBrasil.Application.DTOs;
using CalendarioBrasil.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CalendarioBrasil.Api.Controllers
{
    [ApiController]
    [Route("api/v1/datas-moveis")]
    [Produces("application/json")]
    public sealed class DatasMoveisController : ControllerBase
    {
        private readonly IDatasMoveisService _service;

        public DatasMoveisController(IDatasMoveisService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        [HttpGet]
        [ProducesResponseType(typeof(DatasMoveisDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public ActionResult<DatasMoveisDto> Obter([FromQuery, Range(LimitesConsultaCalendario.AnoMinimo, LimitesConsultaCalendario.AnoMaximo)] int ano)
        {
            return Ok(_service.ObterPorAno(ano));
        }
    }
}
