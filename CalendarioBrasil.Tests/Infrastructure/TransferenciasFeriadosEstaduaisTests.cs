using CalendarioBrasil.Domain.Enums;
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
        [InlineData(2027, "AC_DIA_CATOLICO", 1, 22)]
        [InlineData(2027, "AC_DIA_EVANGELICO", 1, 23)]
        [InlineData(2027, "AC_TRATADO_PETROPOLIS", 11, 19)]
        [InlineData(2028, "AC_DIA_MULHER", 3, 10)]
        [InlineData(2028, "AC_DIA_AMAZONIA", 9, 8)]
        public void Gerar_QuandoNaoHaConflito_DeveAplicarRegraPermanente(int ano, string codigo, int mes, int dia)
        {
            var feriado = Assert.Single(_gerador.Gerar(new AnoCalendario(ano), "AC"), item => item.Codigo == codigo);

            Assert.Equal(new DateOnly(ano, mes, dia), feriado.Data);
            Assert.Equal(SituacaoDataFeriado.Calculada, feriado.SituacaoData);
        }

        [Theory]
        [InlineData("AC_DIA_CATOLICO", 1, 20)]
        [InlineData("AC_TRATADO_PETROPOLIS", 11, 17)]
        public void Gerar_QuandoHaConflitoEm2026_DeveManterReferenciaEInformarPendencia(string codigo, int mes, int dia)
        {
            var feriado = Assert.Single(_gerador.Gerar(new AnoCalendario(2026), "AC"), item => item.Codigo == codigo);

            Assert.Equal(new DateOnly(2026, mes, dia), feriado.Data);
            Assert.Equal(feriado.DataOriginal, feriado.Data);
            Assert.False(feriado.Transferido);
            Assert.Equal(SituacaoDataFeriado.PendenteConfirmacao, feriado.SituacaoData);
        }

        [Fact]
        public void Gerar_DevePreservarDiaDoEvangelicoEm2026()
        {
            var feriado = Assert.Single(_gerador.Gerar(new AnoCalendario(2026), "AC"), item => item.Codigo == "AC_DIA_EVANGELICO");

            Assert.Equal(new DateOnly(2026, 1, 23), feriado.Data);
            Assert.Equal(SituacaoDataFeriado.Calculada, feriado.SituacaoData);
        }

        [Fact]
        public void Gerar_AniversarioDoAcreNaoDeveSerTransferido()
        {
            var feriado = Assert.Single(_gerador.Gerar(new AnoCalendario(2027), "AC"), item => item.Codigo == "AC_ANIVERSARIO_ESTADO");

            Assert.Equal(new DateOnly(2027, 6, 15), feriado.Data);
            Assert.False(feriado.Transferido);
        }

        [Fact]
        public void Gerar_QuandoDataCaiNoDomingo_NaoDeveTransferir()
        {
            var feriado = Assert.Single(_gerador.Gerar(new AnoCalendario(2027), "AC"), item => item.Codigo == "AC_DIA_AMAZONIA");

            Assert.Equal(new DateOnly(2027, 9, 5), feriado.Data);
            Assert.False(feriado.Transferido);
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