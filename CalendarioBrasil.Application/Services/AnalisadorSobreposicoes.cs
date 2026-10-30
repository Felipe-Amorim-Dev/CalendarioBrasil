using CalendarioBrasil.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Application.Services
{
    public static class AnalisadorSobreposicoes
    {
        public static IReadOnlyList<AlertaCalendarioDto> AnalisarFeriados(IEnumerable<FeriadoDto> feriados)
        {
            ArgumentNullException.ThrowIfNull(feriados);

            return Analisar(feriados, item => item.Data, item => item.Codigo, item => item.Abrangencia == "Nacional", item => item.Abrangencia == "Estadual");
        }

        public static IReadOnlyList<AlertaCalendarioDto> AnalisarEventos(IEnumerable<EventoCalendarioDto> eventos)
        {
            ArgumentNullException.ThrowIfNull(eventos);

            return Analisar(eventos.Where(item => item.Tipo == "Feriado"), item => item.Data, item => item.Codigo, item => item.Esfera == "Nacional", item => item.Esfera == "Estadual");
        }

        private static IReadOnlyList<AlertaCalendarioDto> Analisar<T>(IEnumerable<T> itens, Func<T, DateOnly> obterData, Func<T, string> obterCodigo, Func<T, bool> nacional, Func<T, bool> estadual)
        {
            return itens.GroupBy(obterData)
                .Where(grupo => grupo.Any(nacional) && grupo.Any(estadual))
                .OrderBy(grupo => grupo.Key)
                .Select(grupo => new AlertaCalendarioDto(
                    "SOBREPOSICAO_NACIONAL_ESTADUAL",
                    grupo.Key,
                    grupo.Select(obterCodigo).Distinct(StringComparer.Ordinal).OrderBy(codigo => codigo, StringComparer.Ordinal).ToArray(),
                    "Existem feriados nacionais e estaduais na mesma data. Os registros foram preservados."))
                .ToArray();
        }
    }
}
