using CalendarioBrasil.Infrastructure.Calendarios.Models;
using CalendarioBrasil.Infrastructure.Calendarios.Regras;
using CalendarioBrasil.Infrastructure.Calendarios.Validacoes;

namespace CalendarioBrasil.Tests.Infrastructure
{
    public sealed class ValidadorAuditoriaEstadualTests
    {
        private static RegraFeriadoEstadual[] CriarRegras()
        {
            return [RegraFeriadoEstadual.Fixa("RJ_TESTE", "Feriado de teste", 4, 23, new DateOnly(2008, 3, 6), "Lei de teste")];
        }

        [Fact]
        public void Validar_QuandoAuditoriaAusente_DevePermitirCatalogoPendente()
        {
            ValidadorAuditoriaEstadual.Validar(null, CriarRegras(), "RJ");
        }

        [Fact]
        public void Validar_QuandoSituacaoInvalida_DeveLancarExcecao()
        {
            var auditoria = new AuditoriaEstadualJson { Situacao = "Desconhecida" };

            Assert.Throws<InvalidDataException>(() => ValidadorAuditoriaEstadual.Validar(auditoria, CriarRegras(), "RJ"));
        }

        [Fact]
        public void Validar_QuandoAprovadaSemFontes_DeveLancarExcecao()
        {
            var auditoria = new AuditoriaEstadualJson
            {
                Situacao = "Aprovada",
                DataRevisao = new DateOnly(2026, 10, 29),
                Responsavel = "Revisor de teste",
                AnoInicialValidado = 2026,
                AnoFinalValidado = 2027
            };

            Assert.Throws<InvalidDataException>(() => ValidadorAuditoriaEstadual.Validar(auditoria, CriarRegras(), "RJ"));
        }

        [Fact]
        public void Validar_QuandoVigenciaLegalDesconhecida_DeveImpedirAprovacao()
        {
            var regras = new[]
            {
                RegraFeriadoEstadual.Fixa("RJ_TESTE", "Feriado de teste", 4, 23, null, "Lei de teste", inicioEvidencia: new DateOnly(2026, 1, 1))
            };

            var auditoria = new AuditoriaEstadualJson
            {
                Situacao = "Aprovada",
                DataRevisao = new DateOnly(2026, 10, 29),
                Responsavel = "Revisor de teste",
                AnoInicialValidado = 2026,
                AnoFinalValidado = 2027,
                FontesPorCodigo = new Dictionary<string, string> { ["RJ_TESTE"] = "https://example.org/lei-teste" }
            };

            Assert.Throws<InvalidDataException>(() => ValidadorAuditoriaEstadual.Validar(auditoria, regras, "RJ"));
        }
    }
}