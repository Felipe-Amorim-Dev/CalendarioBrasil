using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Infrastructure.Calendarios.Models
{
    internal sealed class AuditoriaEstadualJson
    {
        public string Situacao { get; init; } = "Pendente";
        public DateOnly? DataRevisao { get; init; }
        public string? Responsavel { get; init; }
        public int? AnoInicialValidado { get; init; }
        public int? AnoFinalValidado { get; init; }
        public Dictionary<string, string> FontesPorCodigo { get; init; } = new(StringComparer.Ordinal);
    }
}
