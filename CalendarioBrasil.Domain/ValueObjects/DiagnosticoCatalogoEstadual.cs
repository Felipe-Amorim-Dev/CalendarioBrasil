using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Domain.ValueObjects
{
    public sealed record DiagnosticoCatalogoEstadual(
           string Uf,
           int RegrasCadastradas,
           string Situacao,
           bool Homologado,
           IReadOnlyList<string> RegrasComVigenciaNaoConfirmada,
           IReadOnlyList<string> Limitacoes,
           int? AnoConsultado = null,
           int? AnoInicialValidado = null,
           int? AnoFinalValidado = null,
           DateOnly? DataRevisao = null,
           string? Responsavel = null,
           IReadOnlyDictionary<string, string>? FontesPorCodigo = null);
}
