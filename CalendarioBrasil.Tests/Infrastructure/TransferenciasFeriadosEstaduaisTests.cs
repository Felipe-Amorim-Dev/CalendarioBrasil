using CalendarioBrasil.Domain.Services;
using CalendarioBrasil.Domain.ValueObjects;
using CalendarioBrasil.Infrastructure.Calendarios;
using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Tests.Infrastructure
{
    public sealed class TransferenciasFeriadosEstaduaisTests
    {
        private readonly GeradorFeriadosEstaduais _gerador = new(new CalculadoraDatasMoveis());

        [Theory]
        [InlineData(2026, "AC_DIA_CATOLICO", 1, 22)]
        [InlineData(2026, "AC_DIA_EVANGELICO", 1, 23)]
        [InlineData(2026, "AC_TRATADO_PETROPOLIS", 11, 19)]
        [InlineData(2027, "AC_DIA_CATOLICO", 1, 22)]
        [InlineData(2027, "AC_DIA_EVANGELICO", 1, 23)]
        [InlineData(2027, "AC_TRATADO_PETROPOLIS", 11, 19)]
        [InlineData(2028, "AC_DIA_MULHER", 3, 10)]
        [InlineData(2028, "AC_DIA_AMAZONIA", 9, 8)]
        public void Gerar_DeveAplicarTransferenciasDoAcre(int ano, string codigo, int mes, int dia)
        {
            var feriados = _gerador.Gerar(new AnoCalendario(ano), "AC");

            Assert.Contains(feriados, feriado => feriado.Codigo == codigo && feriado.Data == new DateOnly(ano, mes, dia));
        }

        [Fact]
        public void Gerar_QuandoAniversarioDoAcreCaiNaTerca_NaoDeveTransferir()
        {
            var feriados = _gerador.Gerar(new AnoCalendario(2027), "AC");

            Assert.Contains(feriados, feriado => feriado.Codigo == "AC_ANIVERSARIO_ESTADO" && feriado.Data == new DateOnly(2027, 6, 15));
        }

        [Fact]
        public void Gerar_QuandoDataCaiNoDomingo_NaoDeveTransferir()
        {
            var feriados = _gerador.Gerar(new AnoCalendario(2027), "AC");

            Assert.Contains(feriados, feriado => feriado.Codigo == "AC_DIA_AMAZONIA" && feriado.Data == new DateOnly(2027, 9, 5));
        }

        [Fact]
        public void Gerar_QuandoExistemDoisFeriadosNaSemana_NaoDeveDuplicarDatasEstaduais()
        {
            var feriados = _gerador.Gerar(new AnoCalendario(2026), "AC");
            var janeiro = feriados.Where(feriado => feriado.Codigo is "AC_DIA_CATOLICO" or "AC_DIA_EVANGELICO").ToArray();

            Assert.Equal(2, janeiro.Length);
            Assert.Equal(2, janeiro.Select(feriado => feriado.Data).Distinct().Count());
        }

        [Fact]
        public void Gerar_QuandoConflitaComFeriadoNacional_DevePreservarFeriadoNacional()
        {
            var feriados = _gerador.Gerar(new AnoCalendario(2026), "AC");
            var tratado = Assert.Single(feriados, feriado => feriado.Codigo == "AC_TRATADO_PETROPOLIS");

            Assert.Equal(new DateOnly(2026, 11, 19), tratado.Data);
        }

        [Fact]
        public void Gerar_QuandoAno2027_DeveManterSeisFeriadosEstaduais()
        {
            var feriados = _gerador.Gerar(new AnoCalendario(2027), "AC");

            Assert.Equal(6, feriados.Count);
            Assert.Equal(6, feriados.Select(feriado => feriado.Codigo).Distinct().Count());
        }

        [Theory]
        [InlineData("RJ", "RJ_CARNAVAL", 2027, 2, 9)]
        [InlineData("SP", "SP_REVOLUCAO_CONSTITUCIONALISTA", 2027, 7, 9)]
        [InlineData("RS", "RS_REVOLUCAO_FARROUPILHA", 2027, 9, 20)]
        [InlineData("SC", "SC_DATA_MAGNA", 2027, 8, 15)]
        public void Gerar_NaoDeveAlterarRegrasDosOutrosEstados(string uf, string codigo, int ano, int mes, int dia)
        {
            var feriados = _gerador.Gerar(new AnoCalendario(ano), uf);

            Assert.Contains(feriados, feriado => feriado.Codigo == codigo && feriado.Data == new DateOnly(ano, mes, dia));
        }

        [Fact]
        public void ObterUfsDisponiveis_DeveContinuarRetornandoVinteESeteUfs()
        {
            Assert.Equal(27, _gerador.ObterUfsDisponiveis().Count);
        }
    }
}
