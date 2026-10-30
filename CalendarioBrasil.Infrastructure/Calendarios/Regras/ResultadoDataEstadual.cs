using CalendarioBrasil.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Infrastructure.Calendarios.Regras
{
    internal sealed record ResultadoDataEstadual(DateOnly Data, SituacaoDataFeriado Situacao);
}
