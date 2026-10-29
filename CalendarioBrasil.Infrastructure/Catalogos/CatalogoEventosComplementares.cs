using CalendarioBrasil.Domain.Entities;
using CalendarioBrasil.Domain.Enums;
using CalendarioBrasil.Domain.Interfaces;
using CalendarioBrasil.Domain.Services;
using CalendarioBrasil.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Infrastructure.Catalogos
{
    public sealed class CatalogoEventosComplementares : ICatalogoEventosComplementares
    {
        private const string PortariaFederal2026 = "Portaria MGI nº 11.460/2025";
        private const string ObservacaoFederal = "Ponto facultativo restrito à Administração Pública Federal abrangida pela portaria.";

        private readonly CalculadoraDatasMoveis _calculadoraMoveis;
        private readonly CalculadoraDatasRecorrentes _calculadoraRecorrentes;

        public CatalogoEventosComplementares(CalculadoraDatasMoveis calculadoraMoveis, CalculadoraDatasRecorrentes calculadoraRecorrentes)
        {
            _calculadoraMoveis = calculadoraMoveis ?? throw new ArgumentNullException(nameof(calculadoraMoveis));
            _calculadoraRecorrentes = calculadoraRecorrentes ?? throw new ArgumentNullException(nameof(calculadoraRecorrentes));
        }

        public IReadOnlyList<EventoCalendario> Gerar(AnoCalendario ano)
        {
            ArgumentNullException.ThrowIfNull(ano);

            var datasMoveis = _calculadoraMoveis.Calcular(ano);
            var diaDasMaes = _calculadoraRecorrentes.Calcular(ano.Valor, 5, DayOfWeek.Sunday, 2);
            var diaDosPais = _calculadoraRecorrentes.Calcular(ano.Valor, 8, DayOfWeek.Sunday, 2);
            var eventos = new List<EventoCalendario>();

            eventos.Add(new EventoCalendario("PASCOA", "Domingo de Páscoa", datasMoveis.Pascoa, TipoEventoCalendario.DataComemorativa, EsferaAplicacaoEvento.Nacional));
            eventos.Add(new EventoCalendario("DIA_DAS_MAES", "Dia das Mães", diaDasMaes, TipoEventoCalendario.DataComemorativa, EsferaAplicacaoEvento.Nacional));
            eventos.Add(new EventoCalendario("DIA_DOS_PAIS", "Dia dos Pais", diaDosPais, TipoEventoCalendario.DataComemorativa, EsferaAplicacaoEvento.Nacional));

            if (ano.Valor == 2026)
            {
                var inicioVigencia = new DateOnly(2026, 1, 1);
                var fimVigencia = new DateOnly(2026, 12, 31);

                eventos.Add(new EventoCalendario("PF_FEDERAL_CARNAVAL_SEGUNDA", "Carnaval - segunda-feira", datasMoveis.Carnaval.AddDays(-1), TipoEventoCalendario.PontoFacultativo, EsferaAplicacaoEvento.AdministracaoPublicaFederal, fundamentoLegal: PortariaFederal2026, vigenciaInicio: inicioVigencia, vigenciaFim: fimVigencia, observacao: ObservacaoFederal));
                eventos.Add(new EventoCalendario("PF_FEDERAL_CARNAVAL_TERCA", "Carnaval - terça-feira", datasMoveis.Carnaval, TipoEventoCalendario.PontoFacultativo, EsferaAplicacaoEvento.AdministracaoPublicaFederal, fundamentoLegal: PortariaFederal2026, vigenciaInicio: inicioVigencia, vigenciaFim: fimVigencia, observacao: ObservacaoFederal));
            }

            return eventos.OrderBy(evento => evento.Data).ThenBy(evento => evento.Codigo).ToArray();
        }
    }
}
