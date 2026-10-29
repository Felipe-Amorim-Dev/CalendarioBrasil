using CalendarioBrasil.Domain.Entities;
using CalendarioBrasil.Domain.Services;
using CalendarioBrasil.Domain.ValueObjects;
using CalendarioBrasil.Infrastructure.Calendarios;
using CalendarioBrasil.Infrastructure.Calendarios.Models;
using CalendarioBrasil.Infrastructure.Calendarios.Regras;
using CalendarioBrasil.Infrastructure.Calendarios.Validacoes;

namespace CalendarioBrasil.Tests.Infrastructure
{
    public sealed class FontesLegaisEstaduaisTests
    {
        private readonly GeradorFeriadosEstaduais _gerador = new(new CalculadoraDatasMoveis());

        [Theory]
        [InlineData("RJ", 3)]
        [InlineData("SP", 1)]
        [InlineData("RS", 1)]
        public void ObterDiagnostico_DeveConterFontesParaTodasAsRegras(string uf, int quantidade)
        {
            var diagnostico = _gerador.ObterDiagnostico(uf, 2027);
            var feriados = _gerador.Gerar(new AnoCalendario(2027), uf);

            Assert.Equal("EmRevisao", diagnostico.Situacao);
            Assert.False(diagnostico.Homologado);
            Assert.Equal(quantidade, diagnostico.RegrasCadastradas);
            Assert.NotNull(diagnostico.FontesPorCodigo);
            Assert.Equal(quantidade, diagnostico.FontesPorCodigo.Count);

            foreach (var feriado in feriados)
            {
                Assert.True(diagnostico.FontesPorCodigo.TryGetValue(feriado.Codigo, out var endereco));
                Assert.True(Uri.TryCreate(endereco, UriKind.Absolute, out var uri));
                Assert.Equal(Uri.UriSchemeHttps, uri.Scheme);
            }
        }

        [Theory]
        [InlineData("RJ", "RJ_CARNAVAL", 2, 9)]
        [InlineData("RJ", "RJ_SAO_JORGE", 4, 23)]
        [InlineData("RJ", "RJ_CORPUS_CHRISTI", 5, 27)]
        [InlineData("SP", "SP_REVOLUCAO_CONSTITUCIONALISTA", 7, 9)]
        [InlineData("RS", "RS_REVOLUCAO_FARROUPILHA", 9, 20)]
        public void Gerar_QuandoAno2027_DeveManterDatasEsperadas(string uf, string codigo, int mes, int dia)
        {
            var feriados = _gerador.Gerar(new AnoCalendario(2027), uf);

            Assert.Contains(feriados, feriado => feriado.Codigo == codigo && feriado.Data == new DateOnly(2027, mes, dia));
        }

        [Fact]
        public void Validar_QuandoFonteNaoCorrespondeRegra_DeveRejeitarMesmoEmRevisao()
        {
            RegraFeriadoEstadual[] regras =
            [
                RegraFeriadoEstadual.Fixa("RJ_TESTE", "Feriado de teste", 4, 23, new DateOnly(2008, 3, 6), "Lei de teste")
            ];

            var auditoria = new AuditoriaEstadualJson
            {
                Situacao = "EmRevisao",
                FontesPorCodigo = new Dictionary<string, string>
                {
                    ["RJ_CODIGO_INEXISTENTE"] = "https://example.org/lei"
                }
            };

            Assert.Throws<InvalidDataException>(() => ValidadorAuditoriaEstadual.Validar(auditoria, regras, "RJ"));
        }

        [Fact]
        public void Validar_QuandoFonteNaoUtilizaHttps_DeveRejeitar()
        {
            RegraFeriadoEstadual[] regras =
            [
                RegraFeriadoEstadual.Fixa("SP_TESTE", "Feriado de teste", 7, 9, new DateOnly(1997, 3, 6), "Lei de teste")
            ];

            var auditoria = new AuditoriaEstadualJson
            {
                Situacao = "EmRevisao",
                FontesPorCodigo = new Dictionary<string, string>
                {
                    ["SP_TESTE"] = "http://example.org/lei"
                }
            };

            Assert.Throws<InvalidDataException>(() => ValidadorAuditoriaEstadual.Validar(auditoria, regras, "SP"));
        }
    }
}