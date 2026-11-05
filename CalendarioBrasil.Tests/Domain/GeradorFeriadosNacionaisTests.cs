using CalendarioBrasil.Domain.Enums;
using CalendarioBrasil.Domain.Services;
using CalendarioBrasil.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Tests.Domain
{
    public sealed class GeradorFeriadosNacionaisTests
    {
        private readonly GeradorFeriadosNacionais _gerador = new(new GeradorFeriadosNacionaisFixos(), new CalculadoraDatasMoveis());

        [Theory]
        [InlineData(2024, 3, 29)]
        [InlineData(2025, 4, 18)]
        [InlineData(2026, 4, 3)]
        [InlineData(2027, 3, 26)]
        [InlineData(2028, 4, 14)]
        public void Gerar_DeveCalcularSextaFeiraSantaCorretamente(int ano, int mes, int dia)
        {
            var feriados = _gerador.Gerar(new AnoCalendario(ano));
            var sextaFeiraSanta = Assert.Single(feriados, feriado => feriado.Codigo == "PAIXAO_CRISTO");

            Assert.Equal(new DateOnly(ano, mes, dia), sextaFeiraSanta.Data);
            Assert.Equal(DayOfWeek.Friday, sextaFeiraSanta.Data.DayOfWeek);
        }

        [Theory]
        [InlineData(2024)]
        [InlineData(2025)]
        [InlineData(2026)]
        [InlineData(2027)]
        [InlineData(2030)]
        [InlineData(2100)]
        public void Gerar_DeveRetornarDezFeriadosNacionaisDeReferencia(int ano)
        {
            var feriados = _gerador.Gerar(new AnoCalendario(ano));

            Assert.Equal(10, feriados.Count);
            Assert.All(feriados, feriado => Assert.Equal(AbrangenciaFeriado.Nacional, feriado.Abrangencia));
            Assert.All(feriados, feriado => Assert.Equal(ano, feriado.Data.Year));
            Assert.Equal(feriados.Count, feriados.Select(feriado => feriado.Codigo).Distinct().Count());
        }

        [Fact]
        public void Gerar_DevePreservarOsNoveFeriadosFixos()
        {
            var ano = new AnoCalendario(2026);
            var fixos = new GeradorFeriadosNacionaisFixos().Gerar(ano);
            var completos = _gerador.Gerar(ano);

            Assert.Equal(9, fixos.Count);
            Assert.Equal(10, completos.Count);

            foreach (var fixo in fixos)
            {
                Assert.Contains(completos, feriado => feriado.Codigo == fixo.Codigo && feriado.Data == fixo.Data);
            }
        }

        [Fact]
        public void Gerar_DeveRetornarFeriadosOrdenados()
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
