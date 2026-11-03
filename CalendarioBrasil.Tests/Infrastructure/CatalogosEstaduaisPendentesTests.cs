using CalendarioBrasil.Domain.Services;
using CalendarioBrasil.Domain.ValueObjects;
using CalendarioBrasil.Infrastructure.Calendarios;
using CalendarioBrasil.Infrastructure.Calendarios.Models;
using CalendarioBrasil.Infrastructure.Calendarios.Regras;
using CalendarioBrasil.Infrastructure.Calendarios.Validacoes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace CalendarioBrasil.Tests.Infrastructure
{
    public sealed class CatalogosEstaduaisPendentesTests
    {
        private readonly GeradorFeriadosEstaduais _gerador = new(new CalculadoraDatasMoveis());

        [Theory]
        [InlineData("GO", 2)]
        [InlineData("MT", 3)]
        [InlineData("PR", 3)]
        public void Catalogo_DevePossuirFontesDocumentais(string uf, int quantidade)
        {
            var assembly = typeof(GeradorFeriadosEstaduais).Assembly;
            using var stream = assembly.GetManifestResourceStream($"CalendarioBrasil.Infrastructure.Catalogos.Estaduais.{uf}.json");

            Assert.NotNull(stream);

            using var documento = JsonDocument.Parse(stream!);
            var raiz = documento.RootElement;
            var auditoria = raiz.GetProperty("auditoria");
            var fontes = auditoria.GetProperty("fontesConsultadas");

            Assert.Equal(uf, raiz.GetProperty("uf").GetString());
            Assert.Equal("EmRevisao", auditoria.GetProperty("situacao").GetString());
            Assert.Empty(raiz.GetProperty("feriados").EnumerateArray());
            Assert.Equal(quantidade, fontes.EnumerateObject().Count());

            foreach (var fonte in fontes.EnumerateObject())
            {
                Assert.False(string.IsNullOrWhiteSpace(fonte.Name));
                Assert.True(Uri.TryCreate(fonte.Value.GetString(), UriKind.Absolute, out var uri));
                Assert.Equal(Uri.UriSchemeHttps, uri.Scheme);
            }
        }

        [Theory]
        [InlineData("GO", "26 de julho")]
        [InlineData("MT", "20 de novembro")]
        [InlineData("PR", "19 de dezembro")]
        public void Diagnostico_DeveExibirObservacoesDocumentais(string uf, string referencia)
        {
            var diagnostico = _gerador.ObterDiagnostico(uf, 2026);

            Assert.Equal(0, diagnostico.RegrasCadastradas);
            Assert.Equal("SemRegrasCadastradas", diagnostico.Situacao);
            Assert.False(diagnostico.Homologado);
            Assert.Contains(diagnostico.Limitacoes, texto => texto.Contains(referencia, StringComparison.OrdinalIgnoreCase));
        }

        [Theory]
        [InlineData("GO")]
        [InlineData("MT")]
        [InlineData("PR")]
        public void Gerar_NaoDeveInventarFeriadosEstaduais(string uf)
        {
            var feriados = _gerador.Gerar(new AnoCalendario(2026), uf);

            Assert.Empty(feriados);
        }

        [Fact]
        public void Validar_QuandoFonteConsultadaNaoUsaHttps_DeveRejeitar()
        {
            var auditoria = new AuditoriaEstadualJson
            {
                Situacao = "EmRevisao",
                FontesConsultadas = new Dictionary<string, string>
                {
                    ["Fonte de teste"] = "http://example.org/lei"
                }
            };

            Assert.Throws<InvalidDataException>(() => ValidadorAuditoriaEstadual.Validar(auditoria, Array.Empty<RegraFeriadoEstadual>(), "MT"));
        }

        [Fact]
        public void Validar_QuandoObservacaoNaoPossuiFonte_DeveRejeitar()
        {
            var auditoria = new AuditoriaEstadualJson
            {
                Situacao = "EmRevisao",
                ObservacaoCobertura = "Observação sem fundamento documental."
            };

            Assert.Throws<InvalidDataException>(() => ValidadorAuditoriaEstadual.Validar(auditoria, Array.Empty<RegraFeriadoEstadual>(), "PR"));
        }
    }
}