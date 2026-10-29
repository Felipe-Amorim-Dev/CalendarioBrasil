using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Application.DTOs
{
    public sealed record CoberturaEstadualDto(
           string Uf,
           int RegrasCadastradas,
           string Situacao,
           bool Homologado,
           IReadOnlyList<string> RegrasComVigenciaNaoConfirmada,
           IReadOnlyList<string> Limitacoes,
           int? AnoConsultado,
           int? AnoInicialValidado,
           int? AnoFinalValidado,
           DateOnly? DataRevisao,
           string? Responsavel,
           IReadOnlyDictionary<string, string>? FontesPorCodigo);
}
