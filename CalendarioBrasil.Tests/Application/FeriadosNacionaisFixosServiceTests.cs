using CalendarioBrasil.Application.Services;
using CalendarioBrasil.Domain.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Tests.Application
{
    public sealed class FeriadosNacionaisFixosServiceTests
    {
        private readonly FeriadosNacionaisFixosService _service = new(new GeradorFeriadosNacionaisFixos());

        [Fact]
        public void ObterPorAno_QuandoAno2027_DeveRetornarNoveFeriados()
        {
            var resultado = _service.ObterPorAno(2027);

            Assert.Equal(9, resultado.Count);
        }

        [Fact]
        public void ObterPorAno_DeveRetornarDatasDoAnoSolicitado()
        {
            var resultado = _service.ObterPorAno(2027);

            Assert.All(resultado, feriado => Assert.Equal(2027, feriado.Data.Year));
        }

        [Fact]
        public void ObterPorAno_DeveRetornarAbrangenciaNacional()
        {
            var resultado = _service.ObterPorAno(2027);

            Assert.All(resultado, feriado => Assert.Equal("Nacional", feriado.Abrangencia));
        }

        [Fact]
        public void ObterPorAno_DeveRetornarFundamentoLegal()
        {
            var resultado = _service.ObterPorAno(2027);

            Assert.All(resultado, feriado => Assert.False(string.IsNullOrWhiteSpace(feriado.FundamentoLegal)));
        }

        [Fact]
        public void ObterPorAno_QuandoAnoInvalido_DeveLancarExcecao()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _service.ObterPorAno(2023));
        }
    }
}
