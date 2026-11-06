using CalendarioBrasil.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Application.Interfaces
{
    public interface IDatabricksIntegrationService
    {
        bool Configurado { get; }
        Task<DatabricksTesteDto> TestarAsync(CancellationToken cancellationToken = default);
    }
}
