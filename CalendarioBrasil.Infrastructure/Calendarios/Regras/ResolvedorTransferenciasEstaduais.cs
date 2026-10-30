using CalendarioBrasil.Domain.Services;
using CalendarioBrasil.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Infrastructure.Calendarios.Regras
{
    internal static class ResolvedorTransferenciasEstaduais
    {
        public static IReadOnlyDictionary<string, DateOnly> Resolver(AnoCalendario ano, IReadOnlyList<(RegraFeriadoEstadual Regra, DateOnly Original, DateOnly Prevista)> ocorrencias)
        {
            ArgumentNullException.ThrowIfNull(ano);
            ArgumentNullException.ThrowIfNull(ocorrencias);

            var resultados = new Dictionary<string, DateOnly>(StringComparer.Ordinal);
            var ocupadas = new HashSet<DateOnly>(new GeradorFeriadosNacionaisFixos().Gerar(ano).Select(feriado => feriado.Data));

            foreach (var ocorrencia in ocorrencias.Where(item => !DeveTransferirSexta(item.Regra, item.Original)))
            {
                resultados.Add(ocorrencia.Regra.Codigo, ocorrencia.Prevista);
                ocupadas.Add(ocorrencia.Prevista);
            }

            foreach (var ocorrencia in ocorrencias.Where(item => DeveTransferirSexta(item.Regra, item.Original)).OrderByDescending(item => item.Original))
            {
                var data = ocorrencia.Prevista;

                while (data >= ocorrencia.Original && ocupadas.Contains(data))
                {
                    data = data.AddDays(-1);
                }

                if (data < ocorrencia.Original)
                {
                    throw new InvalidDataException($"Não foi possível resolver a transferência do feriado {ocorrencia.Regra.Codigo}.");
                }

                resultados.Add(ocorrencia.Regra.Codigo, data);
                ocupadas.Add(data);
            }

            return resultados;
        }

        private static bool DeveTransferirSexta(RegraFeriadoEstadual regra, DateOnly original)
        {
            return regra.TransferirSextaFeira && original.DayOfWeek >= DayOfWeek.Tuesday && original.DayOfWeek <= DayOfWeek.Thursday;
        }
    }
}
