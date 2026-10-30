using CalendarioBrasil.Domain.Enums;
using CalendarioBrasil.Domain.Services;
using CalendarioBrasil.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Infrastructure.Calendarios.Regras
{
    internal static class ResolvedorTransferenciasEstaduais
    {
        public static IReadOnlyDictionary<string, ResultadoDataEstadual> Resolver(AnoCalendario ano, IReadOnlyList<(RegraFeriadoEstadual Regra, DateOnly Original, DateOnly Prevista)> ocorrencias, IReadOnlyList<ExcecaoDataEstadual> excecoes, EscopoCalendarioEstadual escopo = EscopoCalendarioEstadual.Geral)
        {
            ArgumentNullException.ThrowIfNull(ano);
            ArgumentNullException.ThrowIfNull(ocorrencias);
            ArgumentNullException.ThrowIfNull(excecoes);

            if (!Enum.IsDefined(escopo))
            {
                throw new ArgumentOutOfRangeException(nameof(escopo));
            }

            var nomeEscopo = escopo.ToString();
            var aplicaveis = excecoes.Where(item => item.Escopo == "Geral" || item.Escopo == nomeEscopo).ToArray();
            var selecionadas = aplicaveis.GroupBy(item => item.Codigo, StringComparer.Ordinal)
                .ToDictionary(grupo => grupo.Key, grupo => grupo.FirstOrDefault(item => item.Escopo == nomeEscopo) ?? grupo.Single(item => item.Escopo == "Geral"), StringComparer.Ordinal);

            var resultados = new Dictionary<string, ResultadoDataEstadual>(StringComparer.Ordinal);
            var ocupadas = new HashSet<DateOnly>(new GeradorFeriadosNacionaisFixos().Gerar(ano).Select(item => item.Data));
            var codigos = ocorrencias.Select(item => item.Regra.Codigo).ToHashSet(StringComparer.Ordinal);

            if (selecionadas.Keys.Any(codigo => !codigos.Contains(codigo)))
            {
                throw new InvalidDataException("Existe exceção aplicável sem ocorrência estadual correspondente.");
            }

            foreach (var ocorrencia in ocorrencias)
            {
                if (selecionadas.TryGetValue(ocorrencia.Regra.Codigo, out var excecao))
                {
                    if (excecao.Ano != ano.Valor || excecao.DataOriginal != ocorrencia.Original)
                    {
                        throw new InvalidDataException($"Exceção inconsistente para {ocorrencia.Regra.Codigo}.");
                    }

                    resultados.Add(ocorrencia.Regra.Codigo, new ResultadoDataEstadual(excecao.DataEfetiva, SituacaoDataFeriado.ExcecaoOficial));
                    ocupadas.Add(excecao.DataEfetiva);
                }
                else if (!DeveTransferirParaSexta(ocorrencia.Regra, ocorrencia.Original))
                {
                    resultados.Add(ocorrencia.Regra.Codigo, new ResultadoDataEstadual(ocorrencia.Prevista, SituacaoDataFeriado.Calculada));
                    ocupadas.Add(ocorrencia.Prevista);
                }
            }

            var transferiveis = ocorrencias.Where(item => !selecionadas.ContainsKey(item.Regra.Codigo) && DeveTransferirParaSexta(item.Regra, item.Original)).ToArray();
            var destinosEmDisputa = transferiveis.GroupBy(item => item.Prevista).Where(grupo => grupo.Count() > 1).Select(grupo => grupo.Key).ToHashSet();

            foreach (var ocorrencia in transferiveis)
            {
                if (ocupadas.Contains(ocorrencia.Prevista) || destinosEmDisputa.Contains(ocorrencia.Prevista))
                {
                    resultados.Add(ocorrencia.Regra.Codigo, new ResultadoDataEstadual(ocorrencia.Original, SituacaoDataFeriado.PendenteConfirmacao));
                }
                else
                {
                    resultados.Add(ocorrencia.Regra.Codigo, new ResultadoDataEstadual(ocorrencia.Prevista, SituacaoDataFeriado.Calculada));
                    ocupadas.Add(ocorrencia.Prevista);
                }
            }

            return resultados;
        }

        private static bool DeveTransferirParaSexta(RegraFeriadoEstadual regra, DateOnly original)
        {
            return regra.TransferirSextaFeira && original.DayOfWeek is >= DayOfWeek.Tuesday and <= DayOfWeek.Thursday;
        }
    }
}