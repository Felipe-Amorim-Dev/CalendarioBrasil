using CalendarioBrasil.Application.Constants;
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

        public CalendarioFeriadosDto ObterPorAnoEUf(int ano, string uf, string escopo = "Geral")
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(uf);
            return ObterPorAno(ano, [uf], escopo);
        }

        public CalendarioFeriadosDto ObterPorAno(int ano, IEnumerable<string>? ufs = null, string escopo = "Geral")
        {
            var anoCalendario = new AnoCalendario(ano);
            var escopoValidado = ConversorEscopoCalendario.Converter(escopo);

            var solicitadas = (ufs ?? Array.Empty<string>())
                .SelectMany(valor => (valor ?? string.Empty).Split(',', StringSplitOptions.None))
                .Select(valor => valor.Trim().ToUpperInvariant())
                .ToArray();

            if (solicitadas.Any(uf => uf.Length != 2 || uf.Any(caractere => caractere is < 'A' or > 'Z') || !SuportaUf(uf)))
            {
                throw new ArgumentException("Uma ou mais UFs informadas são inválidas ou não estão disponíveis.", nameof(ufs));
            }

            var normalizadas = solicitadas.Distinct(StringComparer.Ordinal).OrderBy(uf => uf, StringComparer.Ordinal).ToArray();
            var nacionais = _geradorNacional.Gerar(anoCalendario);
            var estaduais = normalizadas.SelectMany(uf => _geradorEstadual.Gerar(anoCalendario, uf, escopoValidado)).ToArray();

            var feriados = nacionais.Concat(estaduais)
                .DistinctBy(feriado => (feriado.Codigo, feriado.Data, feriado.Abrangencia, feriado.Uf))
                .OrderBy(feriado => feriado.Data)
                .ThenBy(feriado => feriado.Codigo)
                .ThenBy(feriado => feriado.Uf)
                .Select(feriado => new FeriadoDto(feriado.Codigo, feriado.Nome, feriado.Data, feriado.Abrangencia.ToString(), feriado.Uf, feriado.FundamentoLegal, feriado.Transferido ? feriado.DataOriginal : null, feriado.Transferido, feriado.SituacaoData.ToString()))
                .ToArray();

            var limitacoes = new List<string>
            {
                "A cobertura legal ainda é parcial e não inclui feriados municipais."
            };

            if (normalizadas.Length == 0)
            {
                limitacoes.Add("Nenhuma UF informada. A consulta contém somente os feriados nacionais cadastrados.");
            }
            else
            {
                if (normalizadas.Any(uf => !_geradorEstadual.ObterDiagnostico(uf, ano).Homologado))
                {
                    limitacoes.Add("Uma ou mais UFs possuem catálogos estaduais ainda não homologados.");
                }

                if (escopoValidado != EscopoCalendarioEstadual.Geral)
                {
                    limitacoes.Add("O escopo institucional aplica somente as exceções documentadas, não incluindo automaticamente recessos e pontos facultativos.");
                }

                if (estaduais.Any(feriado => feriado.SituacaoData == SituacaoDataFeriado.PendenteConfirmacao))
                {
                    limitacoes.Add("Existem feriados estaduais com transferência de data pendente de confirmação.");
                }
            }

            return new CalendarioFeriadosDto(ano, normalizadas.Length == 1 ? normalizadas[0] : null, "Parcial", feriados.Length, feriados, limitacoes)
            {
                Ufs = normalizadas,
                Escopo = escopoValidado.ToString(),
                Alertas = AnalisadorSobreposicoes.AnalisarFeriados(feriados)
            };
        }
    }
}