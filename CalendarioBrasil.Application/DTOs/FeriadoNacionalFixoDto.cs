using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Application.DTOs
{
    public sealed record FeriadoNacionalFixoDto(
        string Codigo,
        string Nome,
        DateOnly Data,
        string Abrangencia,
        string FundamentoLegal
    );
}
