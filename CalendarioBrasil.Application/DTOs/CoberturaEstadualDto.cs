using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Application.DTOs
{
    public sealed record CoberturaEstadualDto(string Uf, int RegrasCadastradas, string Situacao, bool Homologado, IReadOnlyList<string> RegrasComVigenciaNaoConfirmada, IReadOnlyList<string> Limitacoes);
}
