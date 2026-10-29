using CalendarioBrasil.Domain.Enums;
using CalendarioBrasil.Domain.Services;
using CalendarioBrasil.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Tests.Domain
{
    public sealed class GeradorFeriadosNacionaisFixosTests
    {
        private readonly GeradorFeriadosNacionaisFixos _gerador = new();

        [Theory]
        [InlineData(2026)]
        [InlineData(2027)]
        [InlineData(2028)]
        public void Gerar_QuandoAnoValido_DeveRetornarNoveFeriados(int ano)
        {
            var feriados = _gerador.Gerar(new AnoCalendario(ano));

            Assert.Equal(9, feriados.Count);
        }

        [Theory]
        [InlineData(1, 1, "CONFRATERNIZACAO_UNIVERSAL")]
        [InlineData(4, 21, "TIRADENTES")]
        [InlineData(5, 1, "DIA_TRABALHO")]
        [InlineData(9, 7, "INDEPENDENCIA_BRASIL")]
        [InlineData(10, 12, "NOSSA_SENHORA_APARECIDA")]
        [InlineData(11, 2, "FINADOS")]
        [InlineData(11, 15, "PROCLAMACAO_REPUBLICA")]
        [InlineData(11, 20, "CONSCIENCIA_NEGRA")]
        [InlineData(12, 25, "NATAL")]
        public void Gerar_QuandoAno2027_DeveRetornarDataCorreta(int mes, int dia, string codigo)
        {
            var feriados = _gerador.Gerar(new AnoCalendario(2027));

            Assert.Contains(feriados, feriado => feriado.Codigo == codigo && feriado.Data == new DateOnly(2027, mes, dia));
        }

        [Fact]
        public void Gerar_DeveRetornarApenasFeriadosNacionais()
        {
            var feriados = _gerador.Gerar(new AnoCalendario(2027));

            Assert.All(feriados, feriado =>
            {
                Assert.Equal(AbrangenciaFeriado.Nacional, feriado.Abrangencia);
                Assert.Null(feriado.Uf);
                Assert.False(string.IsNullOrWhiteSpace(feriado.FundamentoLegal));
            });
        }

        [Fact]
        public void Gerar_NaoDeveRetornarCodigosDuplicados()
        {
            var feriados = _gerador.Gerar(new AnoCalendario(2027));

            Assert.Equal(feriados.Count, feriados.Select(feriado => feriado.Codigo).Distinct().Count());
        }

        [Fact]
        public void Gerar_DeveRetornarFeriadosOrdenadosPorData()
        {
            var feriados = _gerador.Gerar(new AnoCalendario(2027));
            var datas = feriados.Select(feriado => feriado.Data).ToArray();

            Assert.True(datas.SequenceEqual(datas.OrderBy(data => data)));
        }

        [Fact]
        public void Gerar_QuandoAnoNulo_DeveLancarExcecao()
        {
            Assert.Throws<ArgumentNullException>(() => _gerador.Gerar(null!));
        }
    }
}
