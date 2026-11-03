using CalendarioBrasil.Application.Services;
using CalendarioBrasil.Domain.Enums;
using CalendarioBrasil.Domain.Services;
using CalendarioBrasil.Domain.ValueObjects;
using CalendarioBrasil.Infrastructure.Calendarios;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace CalendarioBrasil.Tests.Infrastructure
{
    public sealed class ExcecoesTocantins2026Tests
    {
        private readonly GeradorFeriadosEstaduais _gerador = new(new CalculadoraDatasMoveis());

        [Theory]
        [InlineData(EscopoCalendarioEstadual.Geral, 9, true)]
        [InlineData(EscopoCalendarioEstadual.ExecutivoEstadual, 9, true)]
        [InlineData(EscopoCalendarioEstadual.JudiciarioEstadual, 5, false)]
        public void Gerar_QuandoAno2026_DeveRespeitarEscopo(EscopoCalendarioEstadual escopo, int dia, bool transferido)
        {
            var feriado = Assert.Single(_gerador.Gerar(new AnoCalendario(2026), "TO", escopo), item => item.Codigo == "TO_CRIACAO_ESTADO");

            Assert.Equal(new DateOnly(2026, 10, dia), feriado.Data);
            Assert.Equal(new DateOnly(2026, 10, 5), feriado.DataOriginal);
            Assert.Equal(transferido, feriado.Transferido);
            Assert.Equal(SituacaoDataFeriado.ExcecaoOficial, feriado.SituacaoData);
        }

        [Theory]
        [InlineData(2025)]
        [InlineData(2027)]
        public void Gerar_QuandoOutroAno_DevePreservarDataOriginal(int ano)
        {
            var escopos = Enum.GetValues<EscopoCalendarioEstadual>();

            foreach (var escopo in escopos)
            {
                var feriado = Assert.Single(_gerador.Gerar(new AnoCalendario(ano), "TO", escopo), item => item.Codigo == "TO_CRIACAO_ESTADO");

                Assert.Equal(new DateOnly(ano, 10, 5), feriado.Data);
                Assert.False(feriado.Transferido);
                Assert.Equal(SituacaoDataFeriado.Calculada, feriado.SituacaoData);
            }
        }

        [Fact]
        public void Gerar_QuandoAno2026_NaoDeveAlterarOutrosFeriados()
        {
            var feriados = _gerador.Gerar(new AnoCalendario(2026), "TO");

            Assert.Contains(feriados, item => item.Codigo == "TO_NOSSA_SENHORA_NATIVIDADE" && item.Data == new DateOnly(2026, 9, 8));
            Assert.Contains(feriados, item => item.Codigo == "TO_SENHOR_BONFIM" && item.Data == new DateOnly(2026, 8, 15));
            Assert.Equal(3, feriados.Count);
        }

        [Theory]
        [InlineData("Geral", 9, true)]
        [InlineData("ExecutivoEstadual", 9, true)]
        [InlineData("JudiciarioEstadual", 5, false)]
        public void CalendarioService_DevePropagarExcecaoPorEscopo(string escopo, int dia, bool transferido)
        {
            var service = new CalendarioService(new GeradorFeriadosNacionaisFixos(), _gerador);
            var resultado = service.ObterPorAnoEUf(2026, "TO", escopo);
            var feriado = Assert.Single(resultado.Feriados, item => item.Codigo == "TO_CRIACAO_ESTADO");

            Assert.Equal(escopo, resultado.Escopo);
            Assert.Equal("Parcial", resultado.Cobertura);
            Assert.Equal(new DateOnly(2026, 10, dia), feriado.Data);
            Assert.Equal(transferido, feriado.Transferido);
            Assert.Equal(transferido ? new DateOnly(2026, 10, 5) : null, feriado.DataOriginal);
            Assert.Equal("ExcecaoOficial", feriado.SituacaoData);
        }

        [Fact]
        public void CatalogoAnual_DeveConterDuasExcecoesComFontesDocumentais()
        {
            var assembly = typeof(GeradorFeriadosEstaduais).Assembly;
            using var stream = assembly.GetManifestResourceStream("CalendarioBrasil.Infrastructure.Catalogos.Excecoes.TO-2026.json");

            Assert.NotNull(stream);

            using var documento = JsonDocument.Parse(stream!);
            var raiz = documento.RootElement;
            var excecoes = raiz.GetProperty("excecoes").EnumerateArray().ToArray();

            Assert.Equal("TO", raiz.GetProperty("uf").GetString());
            Assert.Equal(2026, raiz.GetProperty("ano").GetInt32());
            Assert.Equal(2, excecoes.Length);
            Assert.Equal(2, excecoes.Select(item => item.GetProperty("escopo").GetString()).Distinct().Count());

            foreach (var excecao in excecoes)
            {
                Assert.Equal("TO_CRIACAO_ESTADO", excecao.GetProperty("codigo").GetString());
                Assert.Equal("2026-10-05", excecao.GetProperty("dataOriginal").GetString());
                Assert.False(string.IsNullOrWhiteSpace(excecao.GetProperty("fundamentoLegal").GetString()));

                var fonte = excecao.GetProperty("fonteOficial").GetString();

                Assert.True(Uri.TryCreate(fonte, UriKind.Absolute, out var uri));
                Assert.Equal(Uri.UriSchemeHttps, uri.Scheme);
            }
        }

        [Fact]
        public void Diagnostico_QuandoAno2026_NaoDeveHomologarTocantins()
        {
            var diagnostico = _gerador.ObterDiagnostico("TO", 2026);

            Assert.False(diagnostico.Homologado);
            Assert.Contains("TO_CRIACAO_ESTADO", diagnostico.RegrasComVigenciaNaoConfirmada);
        }

        [Fact]
        public void Gerar_DevePreservarExcecoesExistentesDoAcre()
        {
            var feriados = _gerador.Gerar(new AnoCalendario(2026), "AC", EscopoCalendarioEstadual.JudiciarioEstadual);
            var tratado = Assert.Single(feriados, item => item.Codigo == "AC_TRATADO_PETROPOLIS");

            Assert.Equal(new DateOnly(2026, 11, 19), tratado.Data);
            Assert.Equal(new DateOnly(2026, 11, 17), tratado.DataOriginal);
            Assert.True(tratado.Transferido);
        }
    }
}