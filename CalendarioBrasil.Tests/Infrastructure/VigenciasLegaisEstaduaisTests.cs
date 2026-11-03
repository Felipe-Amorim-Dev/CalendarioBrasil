using CalendarioBrasil.Domain.Services;
using CalendarioBrasil.Domain.ValueObjects;
using CalendarioBrasil.Infrastructure.Calendarios;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace CalendarioBrasil.Tests.Infrastructure
{
    public sealed class VigenciasLegaisEstaduaisTests
    {
        private readonly GeradorFeriadosEstaduais _gerador = new(new CalculadoraDatasMoveis());

        [Fact]
        public void Vigencia_QuandoNossaSenhoraDaPenha_DeveIniciarEmJulhoDe2019()
        {
            using var documento = CarregarCatalogo("ES");
            var regra = ObterRegra(documento, "ES_NOSSA_SENHORA_PENHA");

            Assert.Equal("2019-07-04", regra.GetProperty("vigenciaInicio").GetString());
            Assert.False(regra.TryGetProperty("inicioEvidencia", out _));
        }

        [Fact]
        public void Gerar_QuandoAno2024_DeveManterFeriadoDaPenha()
        {
            var feriados = _gerador.Gerar(new AnoCalendario(2024), "ES");

            Assert.Contains(feriados, feriado => feriado.Codigo == "ES_NOSSA_SENHORA_PENHA" && feriado.Data == new DateOnly(2024, 4, 8));
        }

        [Fact]
        public void Diagnostico_QuandoEspiritoSanto_NaoDeveIndicarVigenciaPendente()
        {
            var diagnostico = _gerador.ObterDiagnostico("ES", 2024);

            Assert.DoesNotContain("ES_NOSSA_SENHORA_PENHA", diagnostico.RegrasComVigenciaNaoConfirmada);
            Assert.False(diagnostico.Homologado);
        }

        [Theory]
        [InlineData("AL", "AL_DIA_EVANGELICO")]
        [InlineData("AP", "AP_DATA_MAGNA")]
        [InlineData("BA", "BA_INDEPENDENCIA")]
        [InlineData("PA", "PA_ADESAO_INDEPENDENCIA")]
        [InlineData("PI", "PI_DIA_PIAUI")]
        [InlineData("RR", "RR_DATA_MAGNA")]
        [InlineData("SE", "SE_INDEPENDENCIA")]
        [InlineData("TO", "TO_NOSSA_SENHORA_NATIVIDADE")]
        [InlineData("TO", "TO_CRIACAO_ESTADO")]
        public void Vigencia_QuandoAindaNaoComprovada_DevePermanecerPendente(string uf, string codigo)
        {
            using var documento = CarregarCatalogo(uf);
            var regra = ObterRegra(documento, codigo);
            var diagnostico = _gerador.ObterDiagnostico(uf, 2026);

            Assert.Equal(JsonValueKind.Null, regra.GetProperty("vigenciaInicio").ValueKind);
            Assert.Contains(codigo, diagnostico.RegrasComVigenciaNaoConfirmada);
            Assert.False(diagnostico.Homologado);
        }

        private static JsonDocument CarregarCatalogo(string uf)
        {
            var assembly = typeof(GeradorFeriadosEstaduais).Assembly;
            using var stream = assembly.GetManifestResourceStream($"CalendarioBrasil.Infrastructure.Catalogos.Estaduais.{uf}.json");

            Assert.NotNull(stream);
            return JsonDocument.Parse(stream!);
        }

        private static JsonElement ObterRegra(JsonDocument documento, string codigo)
        {
            return documento.RootElement.GetProperty("feriados").EnumerateArray().Single(regra => regra.GetProperty("codigo").GetString() == codigo);
        }
    }
}
