using CalendarioBrasil.Domain.Enums;
using CalendarioBrasil.Domain.Services;
using CalendarioBrasil.Domain.ValueObjects;
using CalendarioBrasil.Infrastructure.Calendarios;

namespace CalendarioBrasil.Tests.Infrastructure
{
    public sealed class GeradorFeriadosEstaduaisTests
    {
        private readonly GeradorFeriadosEstaduais _gerador = new(new CalculadoraDatasMoveis());

        [Fact]
        public void ObterUfsDisponiveis_DeveRetornarEstadosCadastrados()
        {
            var ufs = _gerador.ObterUfsDisponiveis();

            Assert.Equal(new[] { "RJ", "RS", "SP" }, ufs);
        }

        [Theory]
        [InlineData("RJ", 2025, 2)]
        [InlineData("RJ", 2026, 3)]
        [InlineData("RJ", 2027, 3)]
        [InlineData("SP", 2027, 1)]
        [InlineData("RS", 2027, 1)]
        public void Gerar_DeveRetornarQuantidadeEsperada(string uf, int ano, int quantidade)
        {
            var feriados = _gerador.Gerar(new AnoCalendario(ano), uf);

            Assert.Equal(quantidade, feriados.Count);
            Assert.All(feriados, feriado => Assert.Equal(uf, feriado.Uf));
            Assert.All(feriados, feriado => Assert.Equal(AbrangenciaFeriado.Estadual, feriado.Abrangencia));
        }

        [Theory]
        [InlineData("RJ", "RJ_CARNAVAL", 2, 9)]
        [InlineData("SP", "SP_REVOLUCAO_CONSTITUCIONALISTA", 7, 9)]
        [InlineData("RS", "RS_REVOLUCAO_FARROUPILHA", 9, 20)]
        public void Gerar_QuandoAno2027_DeveCalcularDatasCorretas(string uf, string codigo, int mes, int dia)
        {
            var feriados = _gerador.Gerar(new AnoCalendario(2027), uf);

            Assert.Contains(feriados, feriado => feriado.Codigo == codigo && feriado.Data == new DateOnly(2027, mes, dia));
        }

        [Fact]
        public void Gerar_QuandoAno2027_DeveCalcularCorpusChristiDoRj()
        {
            var feriados = _gerador.Gerar(new AnoCalendario(2027), "RJ");

            Assert.Contains(feriados, feriado => feriado.Codigo == "RJ_CORPUS_CHRISTI" && feriado.Data == new DateOnly(2027, 5, 27));
        }

        [Fact]
        public void Gerar_QuandoAno2028_DeveCalcularCarnavalBissexto()
        {
            var feriados = _gerador.Gerar(new AnoCalendario(2028), "RJ");

            Assert.Contains(feriados, feriado => feriado.Codigo == "RJ_CARNAVAL" && feriado.Data == new DateOnly(2028, 2, 29));
        }

        [Fact]
        public void Gerar_QuandoAno2025_NaoDeveIncluirCorpusChristiDoRj()
        {
            var feriados = _gerador.Gerar(new AnoCalendario(2025), "RJ");

            Assert.DoesNotContain(feriados, feriado => feriado.Codigo == "RJ_CORPUS_CHRISTI");
        }

        [Theory]
        [InlineData("rj")]
        [InlineData(" RJ ")]
        public void Gerar_DeveNormalizarUf(string uf)
        {
            var feriados = _gerador.Gerar(new AnoCalendario(2027), uf);

            Assert.Equal(3, feriados.Count);
            Assert.All(feriados, feriado => Assert.Equal("RJ", feriado.Uf));
        }

        [Theory]
        [InlineData("MG")]
        [InlineData("XX")]
        public void Gerar_QuandoUfNaoCadastrada_DeveLancarExcecao(string uf)
        {
            Assert.Throws<ArgumentException>(() => _gerador.Gerar(new AnoCalendario(2027), uf));
        }

        [Fact]
        public void Gerar_QuandoAnoNulo_DeveLancarExcecao()
        {
            Assert.Throws<ArgumentNullException>(() => _gerador.Gerar(null!, "RJ"));
        }

        [Theory]
        [InlineData("RJ")]
        [InlineData("SP")]
        [InlineData("RS")]
        public void Gerar_NaoDeveRetornarCodigosDuplicados(string uf)
        {
            var feriados = _gerador.Gerar(new AnoCalendario(2027), uf);

            Assert.Equal(feriados.Count, feriados.Select(feriado => feriado.Codigo).Distinct().Count());
            Assert.All(feriados, feriado => Assert.False(string.IsNullOrWhiteSpace(feriado.FundamentoLegal)));
        }
    }
}