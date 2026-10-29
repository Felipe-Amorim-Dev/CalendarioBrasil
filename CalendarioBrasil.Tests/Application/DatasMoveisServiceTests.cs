using CalendarioBrasil.Application.Services;
using CalendarioBrasil.Domain.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Tests.Application
{
    public sealed class DatasMoveisServiceTests
    {
        private readonly DatasMoveisService _service = new(new CalculadoraDatasMoveis());

        [Fact]
        public void ObterPorAno_QuandoAno2027_DeveRetornarDatasCorretas()
        {
            var resultado = _service.ObterPorAno(2027);

            Assert.Equal(2027, resultado.Ano);
            Assert.Equal(new DateOnly(2027, 2, 9), resultado.Carnaval);
            Assert.Equal(new DateOnly(2027, 2, 10), resultado.QuartaFeiraCinzas);
            Assert.Equal(new DateOnly(2027, 3, 26), resultado.SextaFeiraSanta);
            Assert.Equal(new DateOnly(2027, 3, 28), resultado.Pascoa);
            Assert.Equal(new DateOnly(2027, 5, 27), resultado.CorpusChristi);
        }

        [Fact]
        public void ObterPorAno_QuandoAnoBissexto_DeveCalcularDatasCorretamente()
        {
            var resultado = _service.ObterPorAno(2028);

            Assert.Equal(new DateOnly(2028, 2, 29), resultado.Carnaval);
            Assert.Equal(new DateOnly(2028, 3, 1), resultado.QuartaFeiraCinzas);
        }

        [Theory]
        [InlineData(2023)]
        [InlineData(2101)]
        public void ObterPorAno_QuandoAnoInvalido_DeveLancarExcecao(int ano)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _service.ObterPorAno(ano));
        }

        [Fact]
        public void ObterPorAno_DeveInformarLimitacaoDaClassificacaoLegal()
        {
            var resultado = _service.ObterPorAno(2027);

            Assert.False(string.IsNullOrWhiteSpace(resultado.Observacao));
        }
    }
}
