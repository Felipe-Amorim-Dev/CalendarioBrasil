using CalendarioBrasil.Application.Services;
using CalendarioBrasil.Domain.Services;
using CalendarioBrasil.Infrastructure.Calendarios;
using CalendarioBrasil.Infrastructure.Calendarios.Models;
using CalendarioBrasil.Infrastructure.Calendarios.Regras;
using CalendarioBrasil.Infrastructure.Calendarios.Validacoes;
using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Tests.Infrastructure
{
    public sealed class HomologacaoAnualEstadualTests
    {
        private readonly GeradorFeriadosEstaduais _gerador = new(new CalculadoraDatasMoveis());

        private static RegraFeriadoEstadual[] CriarRegras()
        {
            return [RegraFeriadoEstadual.Fixa("RJ_TESTE", "Feriado de teste", 4, 23, new DateOnly(2008, 3, 6), "Lei de teste")];
        }

        private static AuditoriaEstadualJson CriarAuditoria(params int[] anos)
        {
            return new AuditoriaEstadualJson
            {
                Situacao = "Aprovada",
                DataRevisao = new DateOnly(2026, 10, 29),
                Responsavel = "Revisor de teste",
                AnoInicialValidado = 2026,
                AnoFinalValidado = 2027,
                AnosRevisados = anos.ToList(),
                FontesPorCodigo = new Dictionary<string, string>
                {
                    ["RJ_TESTE"] = "https://example.org/lei-teste"
                }
            };
        }

        [Fact]
        public void Validar_QuandoTodosOsAnosRevisados_DeveAceitarAuditoria()
        {
            var auditoria = CriarAuditoria(2026, 2027);

            var excecao = Record.Exception(() => ValidadorAuditoriaEstadual.Validar(auditoria, CriarRegras(), "RJ"));

            Assert.Null(excecao);
        }

        [Fact]
        public void Validar_QuandoNenhumAnoRevisado_DeveRejeitarAprovacao()
        {
            var auditoria = CriarAuditoria();

            Assert.Throws<InvalidDataException>(() => ValidadorAuditoriaEstadual.Validar(auditoria, CriarRegras(), "RJ"));
        }

        [Fact]
        public void Validar_QuandoFaltaUmAno_DeveRejeitarAprovacao()
        {
            var auditoria = CriarAuditoria(2026);

            Assert.Throws<InvalidDataException>(() => ValidadorAuditoriaEstadual.Validar(auditoria, CriarRegras(), "RJ"));
        }

        [Fact]
        public void Validar_QuandoExistemAnosDuplicados_DeveRejeitar()
        {
            var auditoria = CriarAuditoria(2026, 2026, 2027);

            Assert.Throws<InvalidDataException>(() => ValidadorAuditoriaEstadual.Validar(auditoria, CriarRegras(), "RJ"));
        }

        [Fact]
        public void Validar_QuandoAnoForaDoPeriodo_DeveRejeitar()
        {
            var auditoria = CriarAuditoria(2026, 2027, 2028);

            Assert.Throws<InvalidDataException>(() => ValidadorAuditoriaEstadual.Validar(auditoria, CriarRegras(), "RJ"));
        }

        [Fact]
        public void Validar_QuandoAnoExcedeLimite_DeveRejeitar()
        {
            var auditoria = CriarAuditoria(2026, 2027, 2101);

            Assert.Throws<InvalidDataException>(() => ValidadorAuditoriaEstadual.Validar(auditoria, CriarRegras(), "RJ"));
        }

        [Fact]
        public void Validar_QuandoFaltaFonteLegal_DeveRejeitarAprovacao()
        {
            var auditoria = CriarAuditoria(2026, 2027);
            auditoria.FontesPorCodigo.Clear();

            Assert.Throws<InvalidDataException>(() => ValidadorAuditoriaEstadual.Validar(auditoria, CriarRegras(), "RJ"));
        }

        [Fact]
        public void Validar_QuandoVigenciaNaoConfirmada_DeveRejeitarAprovacao()
        {
            var auditoria = CriarAuditoria(2026, 2027);
            var regras = new[]
            {
                RegraFeriadoEstadual.Fixa("RJ_TESTE", "Feriado de teste", 4, 23, null, "Lei de teste", inicioEvidencia: new DateOnly(2026, 1, 1))
            };

            Assert.Throws<InvalidDataException>(() => ValidadorAuditoriaEstadual.Validar(auditoria, regras, "RJ"));
        }

        [Fact]
        public void Diagnostico_QuandoCatalogosAindaEmRevisao_NaoDeveHomologar()
        {
            var ufs = _gerador.ObterUfsDisponiveis();

            Assert.Equal(27, ufs.Count);

            foreach (var uf in ufs)
            {
                var diagnostico = _gerador.ObterDiagnostico(uf, 2026);

                Assert.False(diagnostico.Homologado);
                Assert.Empty(diagnostico.AnosRevisados ?? []);
            }
        }

        [Fact]
        public void CoberturaEstadualService_DeveExporAnosRevisados()
        {
            var service = new CoberturaEstadualService(_gerador);

            var resultado = service.ObterPorUf("RJ", 2026);

            Assert.Equal("RJ", resultado.Uf);
            Assert.NotNull(resultado.AnosRevisados);
            Assert.Empty(resultado.AnosRevisados);
            Assert.False(resultado.Homologado);
        }
    }
}
