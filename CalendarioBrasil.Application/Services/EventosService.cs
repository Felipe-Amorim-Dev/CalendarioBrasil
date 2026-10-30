using CalendarioBrasil.Application.Constants;
using CalendarioBrasil.Application.DTOs;
using CalendarioBrasil.Application.Interfaces;
using CalendarioBrasil.Domain.Entities;
using CalendarioBrasil.Domain.Enums;
using CalendarioBrasil.Domain.Interfaces;
using CalendarioBrasil.Domain.Services;
using CalendarioBrasil.Domain.ValueObjects;

namespace CalendarioBrasil.Application.Services
{
    public sealed class EventosService : IEventosService
    {
        private readonly GeradorFeriadosNacionaisFixos _geradorNacional;
        private readonly IGeradorFeriadosEstaduais _geradorEstadual;
        private readonly ICatalogoEventosComplementares _catalogoComplementar;

        public EventosService(GeradorFeriadosNacionaisFixos geradorNacional, IGeradorFeriadosEstaduais geradorEstadual, ICatalogoEventosComplementares catalogoComplementar)
        {
            _geradorNacional = geradorNacional ?? throw new ArgumentNullException(nameof(geradorNacional));
            _geradorEstadual = geradorEstadual ?? throw new ArgumentNullException(nameof(geradorEstadual));
            _catalogoComplementar = catalogoComplementar ?? throw new ArgumentNullException(nameof(catalogoComplementar));
        }

        public bool SuportaUf(string? uf)
        {
            return !string.IsNullOrWhiteSpace(uf) && _geradorEstadual.ObterUfsDisponiveis().Contains(uf.Trim(), StringComparer.OrdinalIgnoreCase);
        }

        public IReadOnlyList<string> ObterUfsSuportadas()
        {
            return _geradorEstadual.ObterUfsDisponiveis();
        }

        public CalendarioEventosDto ObterPorAnoEUf(int ano, string uf, bool incluirPontosFacultativosFederais = false, string escopo = "Geral")
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(uf);

            var anoCalendario = new AnoCalendario(ano);
            var ufNormalizada = uf.Trim().ToUpperInvariant();
            var escopoValidado = ConversorEscopoCalendario.Converter(escopo);

            if (!SuportaUf(ufNormalizada))
            {
                throw new ArgumentException($"A UF {ufNormalizada} ainda não possui catálogo estadual disponível.", nameof(uf));
            }

            var estaduais = _geradorEstadual.Gerar(anoCalendario, ufNormalizada, escopoValidado);
            var feriados = _geradorNacional.Gerar(anoCalendario).Concat(estaduais).Select(ConverterFeriado);
            var complementares = _catalogoComplementar.Gerar(anoCalendario)
                .Where(evento => incluirPontosFacultativosFederais || evento.Tipo != TipoEventoCalendario.PontoFacultativo || evento.Esfera != EsferaAplicacaoEvento.AdministracaoPublicaFederal);

            var eventos = feriados.Concat(complementares)
                .DistinctBy(evento => (evento.Codigo, evento.Data, evento.Tipo, evento.Esfera, evento.Uf, evento.CodigoMunicipioIbge))
                .OrderBy(evento => evento.Data).ThenBy(evento => evento.Codigo)
                .Select(evento => new EventoCalendarioDto(evento.Codigo, evento.Nome, evento.Data, evento.Tipo.ToString(), evento.Esfera.ToString(), evento.Uf, evento.CodigoMunicipioIbge, evento.FundamentoLegal, evento.VigenciaInicio, evento.VigenciaFim, evento.Observacao, evento.Transferido ? evento.DataOriginal : null, evento.Transferido, evento.SituacaoData.ToString()))
                .ToArray();

            var limitacoes = new List<string>
            {
                "Cobertura parcial: o catálogo ainda não contém todos os feriados aplicáveis à localidade.",
                "Feriados municipais e outras regras legais ainda não estão completamente implementados.",
                "Datas comemorativas não representam automaticamente feriados ou dias não úteis."
            };

            if (incluirPontosFacultativosFederais)
            {
                limitacoes.Add("Pontos facultativos federais aplicam-se somente aos órgãos abrangidos pelas respectivas normas, independentemente do escopo estadual consultado.");
            }

            if (escopoValidado != EscopoCalendarioEstadual.Geral)
            {
                limitacoes.Add("O escopo institucional aplica exceções documentadas para o órgão selecionado, mas não inclui automaticamente recessos ou pontos facultativos desse órgão.");
            }

            if (estaduais.Any(feriado => feriado.SituacaoData == SituacaoDataFeriado.PendenteConfirmacao))
            {
                limitacoes.Add("Existem feriados estaduais com transferência pendente de confirmação. Para esses registros, a data informada é apenas uma referência provisória, não uma data efetiva confirmada.");
            }

            return new CalendarioEventosDto(anoCalendario.Valor, ufNormalizada, "Parcial", eventos.Length, eventos, limitacoes)
            {
                Escopo = escopoValidado.ToString(),
                Alertas = AnalisadorSobreposicoes.AnalisarEventos(eventos)
            };
        }

        private static EventoCalendario ConverterFeriado(Feriado feriado)
        {
            var esfera = feriado.Abrangencia switch
            {
                AbrangenciaFeriado.Nacional => EsferaAplicacaoEvento.Nacional,
                AbrangenciaFeriado.Estadual => EsferaAplicacaoEvento.Estadual,
                _ => throw new NotSupportedException("A conversão de feriados municipais ainda não está implementada.")
            };

            return new EventoCalendario(feriado.Codigo, feriado.Nome, feriado.Data, TipoEventoCalendario.Feriado, esfera, uf: feriado.Uf, fundamentoLegal: feriado.FundamentoLegal, dataOriginal: feriado.DataOriginal, situacaoData: feriado.SituacaoData);
        }
    }
}