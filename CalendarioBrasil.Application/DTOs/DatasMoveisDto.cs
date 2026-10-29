using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Application.DTOs
{
    public sealed record DatasMoveisDto(int Ano, DateOnly Carnaval, DateOnly QuartaFeiraCinzas, DateOnly SextaFeiraSanta, DateOnly Pascoa, DateOnly CorpusChristi, string Observacao);
}
