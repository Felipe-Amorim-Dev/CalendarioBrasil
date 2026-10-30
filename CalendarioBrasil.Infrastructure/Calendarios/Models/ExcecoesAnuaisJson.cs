using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Infrastructure.Calendarios.Models
{
    internal sealed class ExcecoesAnuaisJson
    {
        public required string Uf { get; init; }
        public required int Ano { get; init; }
        public required List<ExcecaoAnualJson> Excecoes { get; init; }
    }

    internal sealed class ExcecaoAnualJson
    {
        public required string Codigo { get; init; }
        public required DateOnly DataOriginal { get; init; }
        public required DateOnly DataEfetiva { get; init; }
        public required string Escopo { get; init; }
        public required string FundamentoLegal { get; init; }
        public required string FonteOficial { get; init; }
    }
}
