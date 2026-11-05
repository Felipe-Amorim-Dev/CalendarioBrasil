using CalendarioBrasil.Application.Services;
using CalendarioBrasil.Domain.Services;
using CalendarioBrasil.Domain.ValueObjects;
using CalendarioBrasil.Infrastructure.Calendarios;
using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Tests.Application
{
    public sealed class ProjecoesCalendarioTests
    {
        private readonly CalendarioService _service = new(new GeradorFeriadosNacionaisFixos(), new GeradorFeriadosEstaduais(new CalculadoraDatasMoveis()));

        [Theory]
        [InlineData(1900, 4, 13)]
        [InlineData(2026, 4, 3)]
        [InlineData(2130, 3, 24)]
        public void Consultar_QuandoAnoDiferente_DeveCalcularSextaFeiraSanta(int ano, int mes, int dia)
        {
            var resultado = _service.ObterPorAno(ano);
            var feriado = Assert.Single(resultado.Feriados, item => item.Codigo == "PAIXAO_CRISTO");

            Assert.Equal(new DateOnly(ano, mes, dia), feriado.Data);
            Assert.Equal(DayOfWeek.Friday, feriado.Data.DayOfWeek);
        }

        [Theory]
        [InlineData(1583)]
        [InlineData(1900)]
        [InlineData(2026)]
        [InlineData(2130)]
        [InlineData(9999)]
        public void Consultar_SemUf_DeveRetornarSomenteNacionais(int ano)
        {
            var resultado = _service.ObterPorAno(ano);

            Assert.Equal(ano, resultado.Ano);
            Assert.Empty(resultado.Ufs);
            Assert.NotEmpty(resultado.Feriados);
            Assert.All(resultado.Feriados, item => Assert.Equal("Nacional", item.Abrangencia));
            Assert.All(resultado.Feriados, item => Assert.Equal(ano, item.Data.Year));
        }

        [Fact]
        public void Consultar_QuandoAno2130EVariasUfs_DeveRetornarProjecao()
        {
            var resultado = _service.ObterPorAno(2130, ["RJ", "SP", "MG"]);

            Assert.Equal(["MG", "RJ", "SP"], resultado.Ufs);
            Assert.Contains(resultado.Feriados, item => item.Uf == "RJ");
            Assert.Contains(resultado.Feriados, item => item.Uf == "SP");
            Assert.Contains(resultado.Feriados, item => item.Uf == "MG");
            Assert.Contains(resultado.Limitacoes, texto => texto.Contains("projeções", StringComparison.OrdinalIgnoreCase));
            Assert.Equal(resultado.Total, resultado.Feriados.Count);
        }

        [Fact]
        public void Consultar_QuandoAno1900_NaoDeveAplicarLeisEstaduaisPosteriores()
        {
            var resultado = _service.ObterPorAno(1900, ["RJ"]);

            Assert.DoesNotContain(resultado.Feriados, item => item.Abrangencia == "Estadual");
            Assert.Contains(resultado.Limitacoes, texto => texto.Contains("histórica", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void Consultar_QuandoAnoFuturo_DeveInformarProjecao()
        {
            var resultado = _service.ObterPorAno(2130);

            Assert.Contains(resultado.Limitacoes, texto => texto.Contains("Consulta futura", StringComparison.OrdinalIgnoreCase));
        }

        [Theory]
        [InlineData(1582)]
        [InlineData(10000)]
        public void Consultar_QuandoAnoForaDoIntervalo_DeveRejeitar(int ano)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _service.ObterPorAno(ano));
        }

        [Theory]
        [InlineData(1900)]
        [InlineData(2130)]
        [InlineData(9999)]
        public void CalcularPascoa_DeveRetornarDataValida(int ano)
        {
            var resultado = new CalculadoraDatasMoveis().Calcular(new AnoCalendario(ano));

            Assert.InRange(resultado.Pascoa, new DateOnly(ano, 3, 22), new DateOnly(ano, 4, 25));
            Assert.Equal(DayOfWeek.Sunday, resultado.Pascoa.DayOfWeek);
        }
    }
}
