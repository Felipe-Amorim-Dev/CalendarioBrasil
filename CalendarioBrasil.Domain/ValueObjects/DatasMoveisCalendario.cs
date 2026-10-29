using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Domain.ValueObjects
{
    public sealed record DatasMoveisCalendario(DateOnly Carnaval, DateOnly QuartaFeiraCinzas, DateOnly SextaFeiraSanta, DateOnly Pascoa, DateOnly CorpusChristi);
}
