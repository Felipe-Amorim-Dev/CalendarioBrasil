using CalendarioBrasil.Domain.Enums;
using CalendarioBrasil.Domain.Services;
using CalendarioBrasil.Domain.ValueObjects;
using CalendarioBrasil.Infrastructure.Calendarios;
using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Tests.Infrastructure
{
    public sealed class GeradorFeriadosEstaduaisRjTests
    {
        private readonly GeradorFeriadosEstaduaisRj _gerador = new(new CalculadoraDatasMoveis());

        [Theory]
        [InlineData(2024, 2)]
        [InlineData(2025, 2)]
        [InlineData(2026, 3)]
        [InlineData(2027, 3)]
        [InlineData(2028, 3)]
        public void Gerar_DeveRetornarQuantidadeCorretaPorAno(int ano, int quantidadeEsperada)
        {
            var feriados = _gerador.Gerar(new AnoCalendario(ano));

            Assert.Equal(quantidadeEsperada, feriados.Count);
        }

        [Fact]
        public void Gerar_QuandoAno2027_DeveRetornarDatasCorretas()
        {
            var feriados = _gerador.Gerar(new AnoCalendario(2027));

            Assert.Collection(feriados,
                feriado =>
                {
                    Assert.Equal("RJ_CARNAVAL", feriado.Codigo);
                    Assert.Equal(new DateOnly(2027, 2, 9), feriado.Data);
                },
                feriado =>
                {
                    Assert.Equal("RJ_SAO_JORGE", feriado.Codigo);
                    Assert.Equal(new DateOnly(2027, 4, 23), feriado.Data);
                },
                feriado =>
                {
                    Assert.Equal("RJ_CORPUS_CHRISTI", feriado.Codigo);
                    Assert.Equal(new DateOnly(2027, 5, 27), feriado.Data);
                });
        }

        [Fact]
        public void Gerar_QuandoAno2025_NaoDeveIncluirCorpusChristi()
        {
            var feriados = _gerador.Gerar(new AnoCalendario(2025));

            Assert.DoesNotContain(feriados, feriado => feriado.Codigo == "RJ_CORPUS_CHRISTI");
        }

        [Fact]
        public void Gerar_QuandoAno2026_DeveIncluirCorpusChristi()
        {
            var feriados = _gerador.Gerar(new AnoCalendario(2026));

            Assert.Contains(feriados, feriado => feriado.Codigo == "RJ_CORPUS_CHRISTI" && feriado.Data == new DateOnly(2026, 6, 4));
        }

        [Fact]
        public void Gerar_QuandoAno2028_DeveCalcularCarnavalEmAnoBissexto()
        {
            var feriados = _gerador.Gerar(new AnoCalendario(2028));

            Assert.Contains(feriados, feriado => feriado.Codigo == "RJ_CARNAVAL" && feriado.Data == new DateOnly(2028, 2, 29));
        }

        [Fact]
        public void Gerar_DeveRetornarApenasFeriadosEstaduaisDoRj()
        {
            var feriados = _gerador.Gerar(new AnoCalendario(2027));

            Assert.All(feriados, feriado =>
            {
                Assert.Equal(AbrangenciaFeriado.Estadual, feriado.Abrangencia);
                Assert.Equal("RJ", feriado.Uf);
                Assert.False(string.IsNullOrWhiteSpace(feriado.FundamentoLegal));
            });
        }

        [Fact]
        public void Gerar_NaoDeveRetornarCodigosDuplicados()
        {
            var feriados = _gerador.Gerar(new AnoCalendario(2027));

            Assert.Equal(feriados.Count, feriados.Select(feriado => feriado.Codigo).Distinct().Count());
        }

        [Fact]
        public void Gerar_QuandoAnoNulo_DeveLancarExcecao()
        {
            Assert.Throws<ArgumentNullException>(() => _gerador.Gerar(null!));
        }
    }
}
