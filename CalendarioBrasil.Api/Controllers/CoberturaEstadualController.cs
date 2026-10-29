using CalendarioBrasil.Application.Constants;
using CalendarioBrasil.Application.DTOs;
using CalendarioBrasil.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CalendarioBrasil.Api.Controllers
{
    [ApiController]
    [Route("api/v1/cobertura-estadual")]
    [Produces("application/json")]
    public sealed class CoberturaEstadualController : ControllerBase
    {
        private readonly ICoberturaEstadualService _service;

        public CoberturaEstadualController(ICoberturaEstadualService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        [HttpGet]
        [ProducesResponseType(typeof(CoberturaEstadualDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public ActionResult<CoberturaEstadualDto> Obter([FromQuery, Required, RegularExpression("^[a-zA-Z]{2}$")] string? uf, [FromQuery, Range(LimitesConsultaCalendario.AnoMinimo, LimitesConsultaCalendario.AnoMaximo)] int? ano = null)
        {
            if (!_service.SuportaUf(uf))
            {
                ModelState.AddModelError(nameof(uf), "A UF informada não possui catálogo estadual disponível.");
                return ValidationProblem(ModelState);
            }

            return Ok(_service.ObterPorUf(uf!, ano));
        }
    }
}