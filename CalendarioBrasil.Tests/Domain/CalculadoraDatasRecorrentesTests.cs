using CalendarioBrasil.Domain.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Tests.Domain
{
    public sealed class CalculadoraDatasRecorrentesTests
    {
        private readonly CalculadoraDatasRecorrentes _calculadora = new();

        [Theory]
        [InlineData(2026, 5, 10)]
        [InlineData(2027, 5, 9)]
        [InlineData(2028, 5, 14)]
        public void Calcular_DeveRetornarSegundoDomingoDeMaio(int ano, int mes, int dia)
        {
            var resultado = _calculadora.Calcular(ano, 5, DayOfWeek.Sunday, 2);

            Assert.Equal(new DateOnly(ano, mes, dia), resultado);
        }

        [Theory]
        [InlineData(2026, 8, 9)]
        [InlineData(2027, 8, 8)]
        [InlineData(2028, 8, 13)]
        public void Calcular_DeveRetornarSegundoDomingoDeAgosto(int ano, int mes, int dia)
        {
            var resultado = _calculadora.Calcular(ano, 8, DayOfWeek.Sunday, 2);

            Assert.Equal(new DateOnly(ano, mes, dia), resultado);
        }

        [Fact]
        public void Calcular_QuandoQuintaOcorrenciaNaoExiste_DeveLancarExcecao()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _calculadora.Calcular(2027, 2, DayOfWeek.Sunday, 5));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(6)]
        public void Calcular_QuandoOcorrenciaInvalida_DeveLancarExcecao(int ocorrencia)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _calculadora.Calcular(2027, 5, DayOfWeek.Sunday, ocorrencia));
        }
    }
}
