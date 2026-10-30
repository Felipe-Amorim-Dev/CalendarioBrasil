using CalendarioBrasil.Domain.Enums;
using CalendarioBrasil.Domain.Services;
using CalendarioBrasil.Domain.ValueObjects;
using CalendarioBrasil.Infrastructure.Calendarios;
using CalendarioBrasil.Infrastructure.Calendarios.Regras;
using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Tests.Infrastructure
{
    public sealed class ExcecoesAnuaisEstaduaisTests
    {
        [Fact]
        public void Resolver_QuandoExisteExcecaoGeral_DeveAplicarDataDocumentada()
        {
            var regra = RegraFeriadoEstadual.FixaTransferidaSexta("AC_TESTE", "Feriado teste", 11, 17, new DateOnly(2020, 1, 1), "Lei de teste");
            var original = new DateOnly(2026, 11, 17);
            var excecao = new ExcecaoDataEstadual("AC", 2026, regra.Codigo, original, new DateOnly(2026, 11, 19), "Geral", "Ato fictício de teste", "https://example.org/ato");
            var datas = ResolvedorTransferenciasEstaduais.Resolver(new AnoCalendario(2026), [(regra, original, new DateOnly(2026, 11, 20))], [excecao]);

            Assert.Equal(new DateOnly(2026, 11, 19), datas[regra.Codigo].Data);
            Assert.Equal(SituacaoDataFeriado.ExcecaoOficial, datas[regra.Codigo].Situacao);
        }

        [Fact]
        public void Resolver_QuandoExcecaoEInstitucional_NaoDeveAplicarNoCalendarioGeral()
        {
            var regra = RegraFeriadoEstadual.FixaTransferidaSexta("AC_TESTE", "Feriado teste", 11, 17, new DateOnly(2020, 1, 1), "Lei de teste");
            var original = new DateOnly(2026, 11, 17);
            var excecao = new ExcecaoDataEstadual("AC", 2026, regra.Codigo, original, new DateOnly(2026, 11, 19), "JudiciarioEstadual", "Portaria fictícia", "https://example.org/portaria");
            var datas = ResolvedorTransferenciasEstaduais.Resolver(new AnoCalendario(2026), [(regra, original, new DateOnly(2026, 11, 20))], [excecao]);

            Assert.Equal(original, datas[regra.Codigo].Data);
            Assert.Equal(SituacaoDataFeriado.PendenteConfirmacao, datas[regra.Codigo].Situacao);
        }

        [Fact]
        public void Resolver_QuandoHaDoisDestinosIguais_DeveSinalizarAmbosComoPendentes()
        {
            var primeira = RegraFeriadoEstadual.FixaTransferidaSexta("AC_TESTE_1", "Feriado teste 1", 1, 19, new DateOnly(2020, 1, 1), "Lei de teste");
            var segunda = RegraFeriadoEstadual.FixaTransferidaSexta("AC_TESTE_2", "Feriado teste 2", 1, 20, new DateOnly(2020, 1, 1), "Lei de teste");
            var ocorrencias = new (RegraFeriadoEstadual Regra, DateOnly Original, DateOnly Prevista)[]
            {
                (primeira, new DateOnly(2027, 1, 19), new DateOnly(2027, 1, 22)),
                (segunda, new DateOnly(2027, 1, 20), new DateOnly(2027, 1, 22))
            };
            var datas = ResolvedorTransferenciasEstaduais.Resolver(new AnoCalendario(2027), ocorrencias, []);

            Assert.Equal(SituacaoDataFeriado.PendenteConfirmacao, datas[primeira.Codigo].Situacao);
            Assert.Equal(SituacaoDataFeriado.PendenteConfirmacao, datas[segunda.Codigo].Situacao);
        }

        [Fact]
        public void ValidarExcecao_QuandoDataOriginalNaoCorrespondeRegra_DeveRejeitar()
        {
            RegraFeriadoEstadual[] regras = [RegraFeriadoEstadual.FixaTransferidaSexta("AC_TESTE", "Feriado teste", 1, 20, new DateOnly(2020, 1, 1), "Lei de teste")];
            var excecao = new ExcecaoDataEstadual("AC", 2026, "AC_TESTE", new DateOnly(2026, 1, 21), new DateOnly(2026, 1, 22), "Geral", "Ato fictício", "https://example.org/ato");

            Assert.Throws<InvalidDataException>(() => CarregadorExcecoesAnuais.ValidarExcecao(excecao, regras, new DateOnly(2026, 4, 5)));
        }

        [Fact]
        public void ValidarExcecao_QuandoFonteHttp_DeveRejeitar()
        {
            RegraFeriadoEstadual[] regras = [RegraFeriadoEstadual.FixaTransferidaSexta("AC_TESTE", "Feriado teste", 1, 20, new DateOnly(2020, 1, 1), "Lei de teste")];
            var excecao = new ExcecaoDataEstadual("AC", 2026, "AC_TESTE", new DateOnly(2026, 1, 20), new DateOnly(2026, 1, 22), "Geral", "Ato fictício", "http://example.org/ato");

            Assert.Throws<InvalidDataException>(() => CarregadorExcecoesAnuais.ValidarExcecao(excecao, regras, new DateOnly(2026, 4, 5)));
        }

        [Fact]
        public void Gerador_DeveCarregarExcecoesDe2026SemAplicarAsInstitucionais()
        {
            var gerador = new GeradorFeriadosEstaduais(new CalculadoraDatasMoveis());
            var feriado = Assert.Single(gerador.Gerar(new AnoCalendario(2026), "AC"), item => item.Codigo == "AC_TRATADO_PETROPOLIS");

            Assert.Equal(new DateOnly(2026, 11, 17), feriado.Data);
            Assert.Equal(SituacaoDataFeriado.PendenteConfirmacao, feriado.SituacaoData);
        }
    }
}
