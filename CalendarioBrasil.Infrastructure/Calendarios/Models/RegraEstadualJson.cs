using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Infrastructure.Calendarios.Models
{
    internal sealed class RegraEstadualJson
    {
        public required string Codigo { get; init; }
        public required string Nome { get; init; }
        public required string TipoRegra { get; init; }
        public int? Mes { get; init; }
        public int? Dia { get; init; }
        public int? DeslocamentoPascoa { get; init; }
        public DateOnly? VigenciaInicio { get; init; }
        public DateOnly? VigenciaFim { get; init; }
        public DateOnly? InicioEvidencia { get; init; }
        public required string FundamentoLegal { get; init; }
    }
}
