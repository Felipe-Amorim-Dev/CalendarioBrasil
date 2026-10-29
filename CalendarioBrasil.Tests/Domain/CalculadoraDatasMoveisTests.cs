using CalendarioBrasil.Domain.Services;
using CalendarioBrasil.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Tests.Domain
{
    public sealed class CalculadoraDatasMoveisTests
    {
        private readonly CalculadoraDatasMoveis _calculadora = new();

        [Theory]
        [InlineData(2024, 3, 31)]
        [InlineData(2025, 4, 20)]
        [InlineData(2026, 4, 5)]
        [InlineData(2027, 3, 28)]
        [InlineData(2028, 4, 16)]
        [InlineData(2030, 4, 21)]
        [InlineData(2038, 4, 25)]
        public void Calcular_QuandoAnoValido_DeveRetornarPascoaCorreta(int ano, int mes, int dia)
        {
            var resultado = _calculadora.Calcular(new AnoCalendario(ano));

            Assert.Equal(new DateOnly(ano, mes, dia), resultado.Pascoa);
        }

        [Fact]
        public void Calcular_QuandoAno2027_DeveRetornarDatasMoveisCorretas()
        {
            var resultado = _calculadora.Calcular(new AnoCalendario(2027));

            Assert.Equal(new DateOnly(2027, 2, 9), resultado.Carnaval);
            Assert.Equal(new DateOnly(2027, 2, 10), resultado.QuartaFeiraCinzas);
            Assert.Equal(new DateOnly(2027, 3, 26), resultado.SextaFeiraSanta);
            Assert.Equal(new DateOnly(2027, 3, 28), resultado.Pascoa);
            Assert.Equal(new DateOnly(2027, 5, 27), resultado.CorpusChristi);
        }

        [Fact]
        public void Calcular_QuandoAnoBissexto_DeveConsiderarFevereiroCom29Dias()
        {
            var resultado = _calculadora.Calcular(new AnoCalendario(2028));

            Assert.Equal(new DateOnly(2028, 2, 29), resultado.Carnaval);
            Assert.Equal(new DateOnly(2028, 3, 1), resultado.QuartaFeiraCinzas);
        }

        [Theory]
        [InlineData(2024)]
        [InlineData(2027)]
        [InlineData(2038)]
        [InlineData(2100)]
        public void Calcular_DeveRetornarDiasDaSemanaCorretos(int ano)
        {
            var resultado = _calculadora.Calcular(new AnoCalendario(ano));

            Assert.Equal(DayOfWeek.Tuesday, resultado.Carnaval.DayOfWeek);
            Assert.Equal(DayOfWeek.Wednesday, resultado.QuartaFeiraCinzas.DayOfWeek);
            Assert.Equal(DayOfWeek.Friday, resultado.SextaFeiraSanta.DayOfWeek);
            Assert.Equal(DayOfWeek.Sunday, resultado.Pascoa.DayOfWeek);
            Assert.Equal(DayOfWeek.Thursday, resultado.CorpusChristi.DayOfWeek);
        }

        [Fact]
        public void Calcular_ParaTodosOsAnosPermitidos_DeveRespeitarIntervaloDaPascoa()
        {
            for (int ano = AnoCalendario.AnoMinimo; ano <= AnoCalendario.AnoMaximo; ano++)
            {
                var resultado = _calculadora.Calcular(new AnoCalendario(ano));

                Assert.InRange(resultado.Pascoa, new DateOnly(ano, 3, 22), new DateOnly(ano, 4, 25));
                Assert.Equal(DayOfWeek.Sunday, resultado.Pascoa.DayOfWeek);
            }
        }

        [Fact]
        public void Calcular_QuandoAnoNulo_DeveLancarExcecao()
        {
            Assert.Throws<ArgumentNullException>(() => _calculadora.Calcular(null!));
        }
    }
}
