using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Infrastructure.Calendarios.Models
{
    internal sealed class CatalogoEstadualJson
    {
        public required string Uf { get; init; }
        public required List<RegraEstadualJson> Feriados { get; init; }
    }
}
