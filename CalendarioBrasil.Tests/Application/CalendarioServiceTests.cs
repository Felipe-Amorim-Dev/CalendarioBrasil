using CalendarioBrasil.Domain.Interfaces;
using CalendarioBrasil.Application.Services;
using CalendarioBrasil.Domain.Services;
using CalendarioBrasil.Infrastructure.Calendarios;
using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Tests.Application
{
    public sealed class CalendarioServiceTests
    {
        private readonly CalendarioService _service = new(new GeradorFeriadosNacionaisFixos(), new GeradorFeriadosEstaduais(new CalculadoraDatasMoveis()));

        [Fact]
        public void ObterPorAnoEUf_QuandoAno2027ERj_DeveRetornarDozeFeriados()
        {
            var resultado = _service.ObterPorAnoEUf(2027, "RJ");

            Assert.Equal(12, resultado.Total);
            Assert.Equal(9, resultado.Feriados.Count(feriado => feriado.Abrangencia == "Nacional"));
            Assert.Equal(3, resultado.Feriados.Count(feriado => feriado.Abrangencia == "Estadual"));
        }

        [Fact]
        public void ObterPorAnoEUf_DeveRetornarDatasDoAnoSolicitado()
        {
            var resultado = _service.ObterPorAnoEUf(2027, "RJ");

            Assert.All(resultado.Feriados, feriado => Assert.Equal(2027, feriado.Data.Year));
        }

        [Fact]
        public void ObterPorAnoEUf_DeveRetornarFeriadosEstaduaisDoRj()
        {
            var resultado = _service.ObterPorAnoEUf(2027, "RJ");

            Assert.Contains(resultado.Feriados, feriado => feriado.Codigo == "RJ_CARNAVAL" && feriado.Data == new DateOnly(2027, 2, 9));
            Assert.Contains(resultado.Feriados, feriado => feriado.Codigo == "RJ_SAO_JORGE" && feriado.Data == new DateOnly(2027, 4, 23));
            Assert.Contains(resultado.Feriados, feriado => feriado.Codigo == "RJ_CORPUS_CHRISTI" && feriado.Data == new DateOnly(2027, 5, 27));
        }

        [Fact]
        public void ObterPorAnoEUf_QuandoAno2025_NaoDeveIncluirCorpusChristiEstadual()
        {
            var resultado = _service.ObterPorAnoEUf(2025, "RJ");

            Assert.Equal(11, resultado.Total);
            Assert.DoesNotContain(resultado.Feriados, feriado => feriado.Codigo == "RJ_CORPUS_CHRISTI");
        }

        [Fact]
        public void ObterPorAnoEUf_DeveRetornarRegistrosOrdenados()
        {
            var resultado = _service.ObterPorAnoEUf(2027, "RJ");
            var datas = resultado.Feriados.Select(feriado => feriado.Data).ToArray();

            Assert.True(datas.SequenceEqual(datas.OrderBy(data => data)));
        }

        [Fact]
        public void ObterPorAnoEUf_NaoDeveRetornarRegistrosDuplicados()
        {
            var resultado = _service.ObterPorAnoEUf(2027, "RJ");
            var chaves = resultado.Feriados.Select(feriado => (feriado.Codigo, feriado.Data, feriado.Abrangencia, feriado.Uf)).ToArray();

            Assert.Equal(chaves.Length, chaves.Distinct().Count());
        }

        [Fact]
        public void ObterPorAnoEUf_DeveInformarCoberturaParcial()
        {
            var resultado = _service.ObterPorAnoEUf(2027, "RJ");

            Assert.Equal("Parcial", resultado.Cobertura);
            Assert.NotEmpty(resultado.Limitacoes);
        }

        [Fact]
        public void ObterPorAnoEUf_DeveNormalizarUf()
        {
            var resultado = _service.ObterPorAnoEUf(2027, "rj");

            Assert.Equal("RJ", resultado.Uf);
        }

        [Fact]
        public void SuportaUf_DeveAceitarSomenteEstadosCadastrados()
        {
            Assert.True(_service.SuportaUf("RJ"));
            Assert.True(_service.SuportaUf("rj"));
            Assert.True(_service.SuportaUf("SP"));
            Assert.False(_service.SuportaUf("XX"));
            Assert.False(_service.SuportaUf(null));
        }

        [Fact]
        public void ObterPorAnoEUf_QuandoUfNaoSuportada_DeveLancarExcecao()
        {
            Assert.Throws<ArgumentException>(() => _service.ObterPorAnoEUf(2027, "XX"));
        }

        [Fact]
        public void ObterPorAnoEUf_QuandoAnoInvalido_DeveLancarExcecao()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _service.ObterPorAnoEUf(2023, "RJ"));
        }
    }
}
