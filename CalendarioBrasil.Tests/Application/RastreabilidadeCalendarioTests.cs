using CalendarioBrasil.Application.DTOs;
using CalendarioBrasil.Application.Services;
using CalendarioBrasil.Domain.Services;
using CalendarioBrasil.Infrastructure.Calendarios;
using CalendarioBrasil.Infrastructure.Catalogos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace CalendarioBrasil.Tests.Application
{
    public sealed class RastreabilidadeCalendarioTests
    {
        private static CalendarioService CriarCalendarioService()
        {
            return new CalendarioService(new GeradorFeriadosNacionaisFixos(), new GeradorFeriadosEstaduais(new CalculadoraDatasMoveis()));
        }

        private static EventosService CriarEventosService()
        {
            var calculadora = new CalculadoraDatasMoveis();
            var gerador = new GeradorFeriadosEstaduais(calculadora);
            var catalogo = new CatalogoEventosComplementares(calculadora, new CalculadoraDatasRecorrentes());
            return new EventosService(new GeradorFeriadosNacionaisFixos(), gerador, catalogo);
        }

        [Fact]
        public void Calendario_QuandoTransferenciaNaoTemConfirmacao_DeveSinalizarPendencia()
        {
            var calendario = CriarCalendarioService().ObterPorAnoEUf(2026, "AC");
            var feriado = Assert.Single(calendario.Feriados, item => item.Codigo == "AC_DIA_CATOLICO");

            Assert.Equal(new DateOnly(2026, 1, 20), feriado.Data);
            Assert.Null(feriado.DataOriginal);
            Assert.False(feriado.Transferido);
            Assert.Equal("PendenteConfirmacao", feriado.SituacaoData);
            Assert.Contains(calendario.Limitacoes, texto => texto.Contains("pendente", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void Calendario_QuandoTransferenciaECalculada_DevePreservarDataOriginal()
        {
            var calendario = CriarCalendarioService().ObterPorAnoEUf(2027, "AC");
            var feriado = Assert.Single(calendario.Feriados, item => item.Codigo == "AC_DIA_CATOLICO");

            Assert.Equal(new DateOnly(2027, 1, 20), feriado.DataOriginal);
            Assert.Equal(new DateOnly(2027, 1, 22), feriado.Data);
            Assert.True(feriado.Transferido);
            Assert.Equal("Calculada", feriado.SituacaoData);
        }

        [Fact]
        public void Calendario_QuandoFeriadoNaoTransferido_DeveManterIndicadorFalso()
        {
            var feriado = Assert.Single(CriarCalendarioService().ObterPorAnoEUf(2027, "AC").Feriados, item => item.Codigo == "AC_ANIVERSARIO_ESTADO");

            Assert.Null(feriado.DataOriginal);
            Assert.False(feriado.Transferido);
            Assert.Equal("Calculada", feriado.SituacaoData);
        }

        [Fact]
        public void Eventos_DevePreservarSituacaoPendenteAposConversao()
        {
            var calendario = CriarEventosService().ObterPorAnoEUf(2026, "AC");
            var evento = Assert.Single(calendario.Eventos, item => item.Codigo == "AC_DIA_CATOLICO");

            Assert.Equal(new DateOnly(2026, 1, 20), evento.Data);
            Assert.Null(evento.DataOriginal);
            Assert.False(evento.Transferido);
            Assert.Equal("PendenteConfirmacao", evento.SituacaoData);
            Assert.Equal("Estadual", evento.Esfera);
        }

        [Fact]
        public void Eventos_QuandoTransferenciaCalculada_DeveSerializarDataOriginal()
        {
            var calendario = CriarEventosService().ObterPorAnoEUf(2027, "AC");
            var json = JsonSerializer.Serialize(calendario, new JsonSerializerOptions(JsonSerializerDefaults.Web));

            Assert.Contains("\"dataOriginal\":\"2027-01-20\"", json);
            Assert.Contains("\"transferido\":true", json);
            Assert.Contains("\"situacaoData\":\"Calculada\"", json);
            Assert.Contains("\"alertas\":", json);
        }

        [Fact]
        public void Analisador_QuandoFeriadosNacionalEEstadualCoincidem_DeveEmitirAlerta()
        {
            var data = new DateOnly(2027, 7, 9);
            FeriadoDto[] feriados =
            [
                new("N_TESTE", "Feriado nacional de teste", data, "Nacional", null, "Lei nacional de teste"),
                new("E_TESTE", "Feriado estadual de teste", data, "Estadual", "SP", "Lei estadual de teste")
            ];

            var alerta = Assert.Single(AnalisadorSobreposicoes.AnalisarFeriados(feriados));
            Assert.Equal("SOBREPOSICAO_NACIONAL_ESTADUAL", alerta.Codigo);
            Assert.Equal(data, alerta.Data);
            Assert.Equal(2, alerta.CodigosEventos.Count);
        }

        [Fact]
        public void Analisador_QuandoFeriadosNaoCoincidem_NaoDeveEmitirAlerta()
        {
            FeriadoDto[] feriados =
            [
                new("N_TESTE", "Feriado nacional de teste", new DateOnly(2027, 1, 1), "Nacional", null, "Lei nacional de teste"),
                new("E_TESTE", "Feriado estadual de teste", new DateOnly(2027, 7, 9), "Estadual", "SP", "Lei estadual de teste")
            ];

            Assert.Empty(AnalisadorSobreposicoes.AnalisarFeriados(feriados));
        }

        [Fact]
        public void Eventos_DeveContinuarInformandoCoberturaParcial()
        {
            Assert.Equal("Parcial", CriarEventosService().ObterPorAnoEUf(2027, "AC").Cobertura);
        }
    }
}