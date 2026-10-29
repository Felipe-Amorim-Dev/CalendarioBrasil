using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Application.DTOs
{
    public sealed record FeriadoDto(string Codigo, string Nome, DateOnly Data, string Abrangencia, string? Uf, string FundamentoLegal);
}
