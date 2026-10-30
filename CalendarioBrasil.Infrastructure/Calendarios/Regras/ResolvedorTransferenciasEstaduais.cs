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
        public static IReadOnlyDictionary<string, ResultadoDataEstadual> Resolver(AnoCalendario ano, IReadOnlyList<(RegraFeriadoEstadual Regra, DateOnly Original, DateOnly Prevista)> ocorrencias, IReadOnlyList<ExcecaoDataEstadual> excecoes)
        {
            ArgumentNullException.ThrowIfNull(ano);
            ArgumentNullException.ThrowIfNull(ocorrencias);
            ArgumentNullException.ThrowIfNull(excecoes);

            var gerais = excecoes.Where(excecao => excecao.Escopo == "Geral").ToDictionary(excecao => excecao.Codigo, StringComparer.Ordinal);
            var resultados = new Dictionary<string, ResultadoDataEstadual>(StringComparer.Ordinal);
            var ocupadas = new HashSet<DateOnly>(new GeradorFeriadosNacionaisFixos().Gerar(ano).Select(feriado => feriado.Data));
            var codigos = ocorrencias.Select(ocorrencia => ocorrencia.Regra.Codigo).ToHashSet(StringComparer.Ordinal);

            if (gerais.Keys.Any(codigo => !codigos.Contains(codigo)))
            {
                throw new InvalidDataException("Existe exceção geral sem uma ocorrência estadual correspondente.");
            }

            // Reserva primeiro todas as datas que não dependem de resolução de conflito.
            foreach (var ocorrencia in ocorrencias)
            {
                if (gerais.TryGetValue(ocorrencia.Regra.Codigo, out var excecao))
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

            var transferiveis = ocorrencias.Where(ocorrencia => !gerais.ContainsKey(ocorrencia.Regra.Codigo) && DeveTransferirParaSexta(ocorrencia.Regra, ocorrencia.Original)).ToArray();
            var destinosEmDisputa = transferiveis.GroupBy(ocorrencia => ocorrencia.Prevista).Where(grupo => grupo.Count() > 1).Select(grupo => grupo.Key).ToHashSet();

            foreach (var ocorrencia in transferiveis)
            {
                if (ocupadas.Contains(ocorrencia.Prevista) || destinosEmDisputa.Contains(ocorrencia.Prevista))
                {
                    // Não escolhemos outra data: a original é apenas uma referência provisória.
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