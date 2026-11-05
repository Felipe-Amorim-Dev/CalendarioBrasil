using System;
using System.Collections.Generic;
using System.Text;
using CalendarioBrasil.Domain.ValueObjects;

namespace CalendarioBrasil.Tests.Domain
{
    public sealed class AnoCalendarioTests
    {
        [Theory]
        [InlineData(1583)]
        [InlineData(1900)]
        [InlineData(2023)]
        [InlineData(2026)]
        [InlineData(2101)]
        [InlineData(2130)]
        [InlineData(9999)]
        public void Construtor_QuandoAnoValido_DeveCriarInstancia(int ano)
        {
            var resultado = new AnoCalendario(ano);

            Assert.Equal(ano, resultado.Valor);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1582)]
        [InlineData(10000)]
        public void Construtor_QuandoAnoInvalido_DeveLancarExcecao(int ano)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new AnoCalendario(ano));
        }
    }
}