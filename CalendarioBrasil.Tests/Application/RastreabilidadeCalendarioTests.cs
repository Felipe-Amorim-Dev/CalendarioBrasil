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
        public void Calendario_QuandoFeriadoTransferido_DevePreservarDataOriginal()
        {
            var calendario = CriarCalendarioService().ObterPorAnoEUf(2026, "AC");
            var feriado = Assert.Single(calendario.Feriados, item => item.Codigo == "AC_DIA_CATOLICO");

            Assert.Equal(new DateOnly(2026, 1, 20), feriado.DataOriginal);
            Assert.Equal(new DateOnly(2026, 1, 22), feriado.Data);
            Assert.True(feriado.Transferido);
        }

        [Fact]
        public void Calendario_QuandoFeriadoNaoTransferido_DeveManterIndicadorFalso()
        {
            var calendario = CriarCalendarioService().ObterPorAnoEUf(2027, "AC");
            var feriado = Assert.Single(calendario.Feriados, item => item.Codigo == "AC_ANIVERSARIO_ESTADO");

            Assert.Null(feriado.DataOriginal);
            Assert.False(feriado.Transferido);
        }

        [Fact]
        public void Eventos_DevePreservarRastreabilidadeAposConversao()
        {
            var calendario = CriarEventosService().ObterPorAnoEUf(2026, "AC");
            var evento = Assert.Single(calendario.Eventos, item => item.Codigo == "AC_DIA_CATOLICO");

            Assert.Equal(new DateOnly(2026, 1, 20), evento.DataOriginal);
            Assert.Equal(new DateOnly(2026, 1, 22), evento.Data);
            Assert.True(evento.Transferido);
            Assert.Equal("Estadual", evento.Esfera);
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

            var alertas = AnalisadorSobreposicoes.AnalisarFeriados(feriados);
            var alerta = Assert.Single(alertas);

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
        public void Eventos_QuandoFeriadoTransferido_DeveSerializarDataOriginal()
        {
            var calendario = CriarEventosService().ObterPorAnoEUf(2026, "AC");
            var json = JsonSerializer.Serialize(calendario, new JsonSerializerOptions(JsonSerializerDefaults.Web));

            Assert.Contains("\"dataOriginal\":\"2026-01-20\"", json);
            Assert.Contains("\"transferido\":true", json);
            Assert.Contains("\"alertas\":", json);
        }

        [Fact]
        public void Eventos_DeveContinuarInformandoCoberturaParcial()
        {
            var calendario = CriarEventosService().ObterPorAnoEUf(2027, "AC");

            Assert.Equal("Parcial", calendario.Cobertura);
        }
    }
}
