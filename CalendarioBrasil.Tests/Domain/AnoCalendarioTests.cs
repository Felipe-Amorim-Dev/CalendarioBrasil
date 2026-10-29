using System;
using System.Collections.Generic;
using System.Text;
using CalendarioBrasil.Domain.ValueObjects;

namespace CalendarioBrasil.Tests.Domain
{
    public sealed class AnoCalendarioTests
    {
        [Theory]
        [InlineData(1900)]
        [InlineData(2027)]
        [InlineData(2100)]
        public void Construtor_QuandoAnoValido_DeveCriarInstancia(int ano)
        {
            var resultado = new AnoCalendario(ano);

            Assert.Equal(ano, resultado.Valor);
        }

        [Theory]
        [InlineData(1899)]
        [InlineData(2101)]
        public void Construtor_QuandoAnoInvalido_DeveLancarExcecao(int ano)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new AnoCalendario(ano));
        }
    }
}
