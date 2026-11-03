using CalendarioBrasil.Application.Services;
using CalendarioBrasil.Domain.Services;
using CalendarioBrasil.Domain.ValueObjects;
using CalendarioBrasil.Infrastructure.Calendarios;
using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Tests.Application
{
    public sealed class ConsultaFeriadosMultiplasUfsTests
    {
        private readonly CalendarioService _service = new(new GeradorFeriadosNacionaisFixos(), new GeradorFeriadosEstaduais(new CalculadoraDatasMoveis()));

        [Theory]
        [InlineData(2024)]
        [InlineData(2025)]
        [InlineData(2026)]
        [InlineData(2027)]
        [InlineData(2030)]
        public void ObterPorAno_SemUf_DeveRetornarSomenteNacionais(int ano)
        {
            var resultado = _service.ObterPorAno(ano);
            var nacionais = new GeradorFeriadosNacionaisFixos().Gerar(new AnoCalendario(ano));

            Assert.Equal(ano, resultado.Ano);
            Assert.Null(resultado.Uf);
            Assert.Empty(resultado.Ufs);
            Assert.Equal(nacionais.Count, resultado.Total);
            Assert.All(resultado.Feriados, feriado => Assert.Equal("Nacional", feriado.Abrangencia));
            Assert.All(resultado.Feriados, feriado => Assert.Equal(ano, feriado.Data.Year));
        }

        [Fact]
        public void ObterPorAno_ComUmaUf_DeveRetornarNacionaisEEstaduais()
        {
            var resultado = _service.ObterPorAno(2027, ["RJ"]);

            Assert.Equal("RJ", resultado.Uf);
            Assert.Equal(["RJ"], resultado.Ufs);
            Assert.Equal(12, resultado.Total);
            Assert.Equal(9, resultado.Feriados.Count(feriado => feriado.Abrangencia == "Nacional"));
            Assert.Equal(3, resultado.Feriados.Count(feriado => feriado.Uf == "RJ"));
        }

        [Fact]
        public void ObterPorAno_ComVariasUfs_DeveRetornarTodas()
        {
            var resultado = _service.ObterPorAno(2027, ["RJ,SP,MG"]);

            Assert.Null(resultado.Uf);
            Assert.Equal(["MG", "RJ", "SP"], resultado.Ufs);
            Assert.Equal(9, resultado.Feriados.Count(feriado => feriado.Abrangencia == "Nacional"));
            Assert.Contains(resultado.Feriados, feriado => feriado.Uf == "RJ");
            Assert.Contains(resultado.Feriados, feriado => feriado.Uf == "SP");
            Assert.Contains(resultado.Feriados, feriado => feriado.Uf == "MG");
        }

        [Fact]
        public void ObterPorAno_ComParametrosRepetidos_DeveEliminarUfsDuplicadas()
        {
            var resultado = _service.ObterPorAno(2027, ["RJ", "SP", "rj"]);

            Assert.Equal(["RJ", "SP"], resultado.Ufs);
            Assert.Equal(9, resultado.Feriados.Count(feriado => feriado.Abrangencia == "Nacional"));
            Assert.Equal(3, resultado.Feriados.Count(feriado => feriado.Uf == "RJ"));
            Assert.Equal(1, resultado.Feriados.Count(feriado => feriado.Uf == "SP"));
        }

        [Fact]
        public void ObterPorAno_DeveNormalizarSiglas()
        {
            var resultado = _service.ObterPorAno(2027, [" rj , sp "]);

            Assert.Equal(["RJ", "SP"], resultado.Ufs);
        }

        [Fact]
        public void ObterPorAno_NaoDeveDuplicarRegistros()
        {
            var resultado = _service.ObterPorAno(2027, ["RJ", "SP", "MG", "RJ"]);
            var chaves = resultado.Feriados.Select(feriado => (feriado.Codigo, feriado.Data, feriado.Abrangencia, feriado.Uf));

            Assert.Equal(resultado.Total, chaves.Distinct().Count());
        }

        [Fact]
        public void ObterPorAno_DevePreservarFeriadosDiferentesNaMesmaData()
        {
            var resultado = _service.ObterPorAno(2027, ["MG"]);
            var eventos = resultado.Feriados.Where(feriado => feriado.Data == new DateOnly(2027, 4, 21)).ToArray();

            Assert.Contains(eventos, feriado => feriado.Abrangencia == "Nacional");
            Assert.Contains(eventos, feriado => feriado.Codigo == "MG_DATA_MAGNA");
        }

        [Theory]
        [InlineData("XX")]
        [InlineData("RJ,XX")]
        [InlineData("RJ,,SP")]
        [InlineData("123")]
        public void ObterPorAno_QuandoUfInvalida_DeveRejeitar(string ufs)
        {
            Assert.Throws<ArgumentException>(() => _service.ObterPorAno(2027, [ufs]));
        }

        [Fact]
        public void ObterPorAno_QuandoAnoInvalido_DeveRejeitar()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _service.ObterPorAno(2023));
        }

        [Fact]
        public void ObterPorAnoEUf_DevePreservarCompatibilidade()
        {
            var antigo = _service.ObterPorAnoEUf(2027, "RJ");
            var novo = _service.ObterPorAno(2027, ["RJ"]);

            Assert.Equal(antigo.Feriados, novo.Feriados);
            Assert.Equal(antigo.Total, novo.Total);
        }
    }
}
