using CalendarioBrasil.Application.DTOs;
using CalendarioBrasil.Application.Interfaces;
using CalendarioBrasil.Domain.Enums;
using CalendarioBrasil.Domain.Interfaces;
using CalendarioBrasil.Domain.Services;
using CalendarioBrasil.Domain.ValueObjects;

namespace CalendarioBrasil.Application.Services
{
    public sealed class CalendarioService : ICalendarioService
    {
        private readonly GeradorFeriadosNacionaisFixos _geradorNacional;
        private readonly IGeradorFeriadosEstaduais _geradorEstadual;

        public CalendarioService(GeradorFeriadosNacionaisFixos geradorNacional, IGeradorFeriadosEstaduais geradorEstadual)
        {
            _geradorNacional = geradorNacional ?? throw new ArgumentNullException(nameof(geradorNacional));
            _geradorEstadual = geradorEstadual ?? throw new ArgumentNullException(nameof(geradorEstadual));
        }

        public bool SuportaUf(string? uf)
        {
            return !string.IsNullOrWhiteSpace(uf) && _geradorEstadual.ObterUfsDisponiveis().Contains(uf.Trim(), StringComparer.OrdinalIgnoreCase);
        }

        public IReadOnlyList<string> ObterUfsSuportadas()
        {
            return _geradorEstadual.ObterUfsDisponiveis();
        }

        public CalendarioFeriadosDto ObterPorAnoEUf(int ano, string uf)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(uf);

            var anoCalendario = new AnoCalendario(ano);
            var ufNormalizada = uf.Trim().ToUpperInvariant();

            if (!SuportaUf(ufNormalizada))
            {
                throw new ArgumentException($"A UF {ufNormalizada} ainda não possui catálogo estadual disponível.", nameof(uf));
            }

            var estaduais = _geradorEstadual.Gerar(anoCalendario, ufNormalizada);
            var feriados = _geradorNacional.Gerar(anoCalendario)
                .Concat(estaduais)
                .DistinctBy(feriado => (feriado.Codigo, feriado.Data, feriado.Abrangencia, feriado.Uf))
                .OrderBy(feriado => feriado.Data)
                .ThenBy(feriado => feriado.Codigo)
                .Select(feriado => new FeriadoDto(feriado.Codigo, feriado.Nome, feriado.Data, feriado.Abrangencia.ToString(), feriado.Uf, feriado.FundamentoLegal, feriado.Transferido ? feriado.DataOriginal : null, feriado.Transferido, feriado.SituacaoData.ToString()))
                .ToArray();

            var limitacoes = new List<string>
            {
                "O calendário inclui feriados nacionais fixos e os eventos estaduais cadastrados para a UF.",
                "A cobertura legal ainda é parcial e não inclui todos os feriados municipais e eventos aplicáveis."
            };

            if (estaduais.Any(feriado => feriado.SituacaoData == SituacaoDataFeriado.PendenteConfirmacao))
            {
                limitacoes.Add("Existem feriados estaduais com transferência pendente de confirmação. Para esses registros, a data informada é apenas uma referência provisória, não uma data efetiva confirmada.");
            }

            return new CalendarioFeriadosDto(anoCalendario.Valor, ufNormalizada, "Parcial", feriados.Length, feriados, limitacoes)
            {
                Alertas = AnalisadorSobreposicoes.AnalisarFeriados(feriados)
            };
        }
    }
}