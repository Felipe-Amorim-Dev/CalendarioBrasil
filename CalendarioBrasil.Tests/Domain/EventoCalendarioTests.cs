using CalendarioBrasil.Domain.Entities;
using CalendarioBrasil.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Tests.Domain
{
    public sealed class EventoCalendarioTests
    {
        [Fact]
        public void Construtor_QuandoFeriadoSemFundamentoLegal_DeveLancarExcecao()
        {
            Assert.Throws<ArgumentException>(() => new EventoCalendario("TESTE", "Feriado Teste", new DateOnly(2027, 1, 1), TipoEventoCalendario.Feriado, EsferaAplicacaoEvento.Nacional));
        }

        [Fact]
        public void Construtor_QuandoPontoFacultativoSemVigencia_DeveLancarExcecao()
        {
            Assert.Throws<ArgumentException>(() => new EventoCalendario("TESTE", "Ponto Facultativo", new DateOnly(2027, 2, 9), TipoEventoCalendario.PontoFacultativo, EsferaAplicacaoEvento.AdministracaoPublicaFederal, fundamentoLegal: "Norma de teste"));
        }

        [Fact]
        public void Construtor_QuandoDataForaDaVigencia_DeveLancarExcecao()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new EventoCalendario("TESTE", "Ponto Facultativo", new DateOnly(2027, 2, 9), TipoEventoCalendario.PontoFacultativo, EsferaAplicacaoEvento.AdministracaoPublicaFederal, fundamentoLegal: "Norma de teste", vigenciaInicio: new DateOnly(2026, 1, 1), vigenciaFim: new DateOnly(2026, 12, 31)));
        }

        [Fact]
        public void Construtor_QuandoFeriadoEstadualSemUf_DeveLancarExcecao()
        {
            Assert.Throws<ArgumentException>(() => new EventoCalendario("TESTE", "Feriado Estadual", new DateOnly(2027, 4, 23), TipoEventoCalendario.Feriado, EsferaAplicacaoEvento.Estadual, fundamentoLegal: "Lei de teste"));
        }

        [Fact]
        public void Construtor_QuandoDataComemorativa_DevePermitirAusenciaDeLei()
        {
            var evento = new EventoCalendario("DIA_DAS_MAES", "Dia das Mães", new DateOnly(2027, 5, 9), TipoEventoCalendario.DataComemorativa, EsferaAplicacaoEvento.Nacional);

            Assert.Null(evento.FundamentoLegal);
            Assert.Equal(TipoEventoCalendario.DataComemorativa, evento.Tipo);
        }
    }
}
