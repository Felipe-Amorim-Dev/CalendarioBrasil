using CalendarioBrasil.Application.Constants;
using CalendarioBrasil.Application.Services;
using CalendarioBrasil.Domain.Enums;
using CalendarioBrasil.Domain.Services;
using CalendarioBrasil.Domain.ValueObjects;
using CalendarioBrasil.Infrastructure.Calendarios;
using CalendarioBrasil.Infrastructure.Catalogos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace CalendarioBrasil.Tests.Application
{
    public sealed class EscoposCalendarioTests
    {
        private static CalendarioService CriarCalendario()
        {
            return new CalendarioService(new GeradorFeriadosNacionaisFixos(), new GeradorFeriadosEstaduais(new CalculadoraDatasMoveis()));
        }

        private static EventosService CriarEventos()
        {
            var calculadora = new CalculadoraDatasMoveis();
            return new EventosService(new GeradorFeriadosNacionaisFixos(), new GeradorFeriadosEstaduais(calculadora), new CatalogoEventosComplementares(calculadora, new CalculadoraDatasRecorrentes()));
        }

        [Fact]
        public void ObterPorAnoEUf_SemEscopo_DeveUsarGeral()
        {
            var service = CriarCalendario();
            var padrao = service.ObterPorAnoEUf(2026, "AC");
            var explicito = service.ObterPorAnoEUf(2026, "AC", "Geral");

            Assert.Equal("Geral", padrao.Escopo);
            Assert.Equal(explicito.Feriados, padrao.Feriados);
        }

        [Theory]
        [InlineData("Geral", 2026, 11, 17, "PendenteConfirmacao")]
        [InlineData("ExecutivoEstadual", 2026, 11, 17, "ExcecaoOficial")]
        [InlineData("JudiciarioEstadual", 2026, 11, 19, "ExcecaoOficial")]
        public void Calendario_QuandoEscopoDiferente_DeveRespeitarExcecoes(string escopo, int ano, int mes, int dia, string situacao)
        {
            var calendario = CriarCalendario().ObterPorAnoEUf(ano, "AC", escopo);
            var feriado = Assert.Single(calendario.Feriados, item => item.Codigo == "AC_TRATADO_PETROPOLIS");

            Assert.Equal(escopo, calendario.Escopo);
            Assert.Equal(new DateOnly(ano, mes, dia), feriado.Data);
            Assert.Equal(situacao, feriado.SituacaoData);
            Assert.Equal("Parcial", calendario.Cobertura);
        }

        [Theory]
        [InlineData("Geral", 2026, 1, 20, "PendenteConfirmacao")]
        [InlineData("ExecutivoEstadual", 2026, 1, 22, "ExcecaoOficial")]
        [InlineData("JudiciarioEstadual", 2026, 1, 22, "ExcecaoOficial")]
        public void Eventos_QuandoEscopoDiferente_DeveRespeitarExcecoes(string escopo, int ano, int mes, int dia, string situacao)
        {
            var calendario = CriarEventos().ObterPorAnoEUf(ano, "AC", escopo: escopo);
            var evento = Assert.Single(calendario.Eventos, item => item.Codigo == "AC_DIA_CATOLICO");

            Assert.Equal(escopo, calendario.Escopo);
            Assert.Equal(new DateOnly(ano, mes, dia), evento.Data);
            Assert.Equal(situacao, evento.SituacaoData);
            Assert.Equal("Parcial", calendario.Cobertura);
        }

        [Fact]
        public void Eventos_DeveManterCompatibilidadeComParametroDePontosFacultativos()
        {
            var eventos = CriarEventos().ObterPorAnoEUf(2026, "RJ", true);

            Assert.Equal("Geral", eventos.Escopo);
            Assert.Equal(2, eventos.Eventos.Count(item => item.Tipo == "PontoFacultativo"));
        }

        [Fact]
        public void ConversorEscopo_DeveNormalizarNomesERejeitarNumeros()
        {
            Assert.True(ConversorEscopoCalendario.TentarConverter(" judiciarioestadual ", out var escopo));
            Assert.Equal(EscopoCalendarioEstadual.JudiciarioEstadual, escopo);
            Assert.False(ConversorEscopoCalendario.TentarConverter("1", out _));
            Assert.False(ConversorEscopoCalendario.TentarConverter("Municipal", out _));
            Assert.False(ConversorEscopoCalendario.TentarConverter(null, out _));
        }

        [Fact]
        public void Servicos_DevemRejeitarEscopoInvalido()
        {
            Assert.Throws<ArgumentException>(() => CriarCalendario().ObterPorAnoEUf(2026, "AC", "Municipal"));
            Assert.Throws<ArgumentException>(() => CriarEventos().ObterPorAnoEUf(2026, "AC", escopo: "1"));
        }

        [Fact]
        public void Gerador_QuandoEscopoInvalido_DeveRejeitar()
        {
            var gerador = new GeradorFeriadosEstaduais(new CalculadoraDatasMoveis());
            Assert.Throws<ArgumentOutOfRangeException>(() => gerador.Gerar(new AnoCalendario(2026), "AC", (EscopoCalendarioEstadual)999));
        }

        [Fact]
        public void Feriados_DeveSerializarEscopoNoJson()
        {
            var calendario = CriarCalendario().ObterPorAnoEUf(2026, "AC", "JudiciarioEstadual");
            var json = JsonSerializer.Serialize(calendario, new JsonSerializerOptions(JsonSerializerDefaults.Web));

            Assert.Contains("\"escopo\":\"JudiciarioEstadual\"", json);
            Assert.Contains("\"situacaoData\":\"ExcecaoOficial\"", json);
        }

        [Fact]
        public void AusenciaDeExcecaoInstitucional_DevePreservarRegraGeral()
        {
            var service = CriarCalendario();
            var geral = service.ObterPorAnoEUf(2027, "AC");
            var judiciario = service.ObterPorAnoEUf(2027, "AC", "JudiciarioEstadual");

            Assert.Equal(geral.Feriados, judiciario.Feriados);
        }
    }
}
