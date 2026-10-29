using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Application.DTOs
{
    public sealed record CalendarioFeriadosDto(int Ano, string Uf, string Cobertura, int Total, IReadOnlyList<FeriadoDto> Feriados, IReadOnlyList<string> Limitacoes);
}
