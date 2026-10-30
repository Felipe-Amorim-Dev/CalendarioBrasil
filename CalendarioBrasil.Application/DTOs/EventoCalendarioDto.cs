using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Application.DTOs
{
    public sealed record EventoCalendarioDto(string Codigo, string Nome, DateOnly Data, string Tipo, string Esfera, string? Uf, string? CodigoMunicipioIbge, string? FundamentoLegal, DateOnly? VigenciaInicio, DateOnly? VigenciaFim, string? Observacao, DateOnly? DataOriginal = null, bool Transferido = false);
}
