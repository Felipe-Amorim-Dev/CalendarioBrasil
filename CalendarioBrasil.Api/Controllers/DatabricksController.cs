using CalendarioBrasil.Application.DTOs;
using CalendarioBrasil.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace CalendarioBrasil.Api.Controllers
{
    [ApiController]
    [Route("api/v1/databricks")]
    [Produces("application/json")]
    public sealed class DatabricksController : ControllerBase
    {
        private readonly IDatabricksIntegrationService _service;
        private readonly IHostEnvironment _environment;

        public DatabricksController(IDatabricksIntegrationService service, IHostEnvironment environment)
        {
            _service = service;
            _environment = environment;
        }

        [HttpGet("status")]
        [ProducesResponseType(typeof(DatabricksStatusDto), StatusCodes.Status200OK)]
        public ActionResult<DatabricksStatusDto> ObterStatus()
        {
            var configurado = _service.Configurado;
            var testeDisponivel = _environment.IsDevelopment();

            return Ok(new DatabricksStatusDto(
                configurado,
                testeDisponivel,
                configurado ? "Credenciais do Databricks configuradas no backend. A conexão ainda precisa ser validada." :
                    "A integração Databricks não está configurada no backend."
            ));
        }

        [HttpPost("testar")]
        [ProducesResponseType(typeof(DatabricksTesteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DatabricksTesteDto>> Testar(CancellationToken cancellationToken)
        {
            if (!_environment.IsDevelopment())
            {
                return NotFound();
            }

            var resultado = await _service.TestarAsync(cancellationToken);
            return Ok(resultado);
        }
    }
}