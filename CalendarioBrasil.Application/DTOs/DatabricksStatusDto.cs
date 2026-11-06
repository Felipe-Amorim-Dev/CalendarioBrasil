using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Application.DTOs
{
    public sealed record DatabricksStatusDto(bool Configurado, bool TesteDisponivel, string Mensagem);
}
