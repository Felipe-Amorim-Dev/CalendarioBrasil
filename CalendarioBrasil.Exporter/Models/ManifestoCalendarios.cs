using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Exporter.Models
{
    public sealed record ManifestoCalendarios(int Versao, DateTimeOffset GeradoEmUtc, IReadOnlyList<CalendarioPublicado> Calendarios);
}
