using CalendarioBrasil.Domain.Enums;
using CalendarioBrasil.Domain.ValueObjects;
using CalendarioBrasil.Infrastructure.Calendarios.Regras;
using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Tests.Infrastructure
{
    public sealed class PrioridadeExcecoesPorEscopoTests
    {
        [Fact]
        public void Resolver_QuandoExcecaoGeralEInstitucionalExistem_DevePriorizarInstitucional()
        {
            var regra = RegraFeriadoEstadual.FixaTransferidaSexta("AC_TESTE", "Evento de teste", 11, 17, new DateOnly(2000, 1, 1), "Lei de teste");
            var original = new DateOnly(2026, 11, 17);
            var ocorrencias = new[] { (Regra: regra, Original: original, Prevista: new DateOnly(2026, 11, 20)) };
            ExcecaoDataEstadual[] excecoes =
            [
                new("AC", 2026, regra.Codigo, original, new DateOnly(2026, 11, 18), "Geral", "Lei de teste", "https://example.org/geral"),
                new("AC", 2026, regra.Codigo, original, new DateOnly(2026, 11, 19), "JudiciarioEstadual", "Portaria de teste", "https://example.org/jud")
            ];

            var geral = ResolvedorTransferenciasEstaduais.Resolver(new AnoCalendario(2026), ocorrencias, excecoes);
            var judiciario = ResolvedorTransferenciasEstaduais.Resolver(new AnoCalendario(2026), ocorrencias, excecoes, EscopoCalendarioEstadual.JudiciarioEstadual);
            var executivo = ResolvedorTransferenciasEstaduais.Resolver(new AnoCalendario(2026), ocorrencias, excecoes, EscopoCalendarioEstadual.ExecutivoEstadual);

            Assert.Equal(new DateOnly(2026, 11, 18), geral[regra.Codigo].Data);
            Assert.Equal(new DateOnly(2026, 11, 19), judiciario[regra.Codigo].Data);
            Assert.Equal(new DateOnly(2026, 11, 18), executivo[regra.Codigo].Data);
            Assert.All(new[] { geral[regra.Codigo], judiciario[regra.Codigo], executivo[regra.Codigo] }, item => Assert.Equal(SituacaoDataFeriado.ExcecaoOficial, item.Situacao));
        }

        [Fact]
        public void Resolver_QuandoNaoExisteExcecaoAplicavel_DevePreservarPendencia()
        {
            var regra = RegraFeriadoEstadual.FixaTransferidaSexta("AC_TESTE", "Evento de teste", 11, 17, new DateOnly(2000, 1, 1), "Lei de teste");
            var original = new DateOnly(2026, 11, 17);
            var ocorrencias = new[] { (Regra: regra, Original: original, Prevista: new DateOnly(2026, 11, 20)) };
            ExcecaoDataEstadual[] excecoes = [new("AC", 2026, regra.Codigo, original, new DateOnly(2026, 11, 19), "JudiciarioEstadual", "Portaria de teste", "https://example.org/jud")];

            var geral = ResolvedorTransferenciasEstaduais.Resolver(new AnoCalendario(2026), ocorrencias, excecoes);
            var judiciario = ResolvedorTransferenciasEstaduais.Resolver(new AnoCalendario(2026), ocorrencias, excecoes, EscopoCalendarioEstadual.JudiciarioEstadual);

            Assert.Equal(original, geral[regra.Codigo].Data);
            Assert.Equal(SituacaoDataFeriado.PendenteConfirmacao, geral[regra.Codigo].Situacao);
            Assert.Equal(new DateOnly(2026, 11, 19), judiciario[regra.Codigo].Data);
        }
    }
}
