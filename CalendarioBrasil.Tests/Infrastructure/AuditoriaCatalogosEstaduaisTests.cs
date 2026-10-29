using CalendarioBrasil.Domain.Services;
using CalendarioBrasil.Infrastructure.Calendarios;
using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Tests.Infrastructure
{
    public sealed class AuditoriaCatalogosEstaduaisTests
    {
        private readonly GeradorFeriadosEstaduais _gerador = new(new CalculadoraDatasMoveis());

        [Fact]
        public void ObterDiagnostico_DeveRetornarDadosDasVinteESeteUfs()
        {
            var ufs = _gerador.ObterUfsDisponiveis();

            Assert.Equal(27, ufs.Count);

            foreach (var uf in ufs)
            {
                var diagnostico = _gerador.ObterDiagnostico(uf);

                Assert.Equal(uf, diagnostico.Uf);
                Assert.False(diagnostico.Homologado);
                Assert.NotEmpty(diagnostico.Limitacoes);
            }
        }

        [Theory]
        [InlineData("GO")]
        [InlineData("MT")]
        [InlineData("PR")]
        public void ObterDiagnostico_QuandoCatalogoVazio_NaoDeveDeclararCoberturaCompleta(string uf)
        {
            var diagnostico = _gerador.ObterDiagnostico(uf);

            Assert.Equal(0, diagnostico.RegrasCadastradas);
            Assert.Equal("SemRegrasCadastradas", diagnostico.Situacao);
            Assert.False(diagnostico.Homologado);
        }

        [Fact]
        public void ObterDiagnostico_QuandoUfTo_DeveIdentificarVigenciasPendentes()
        {
            var diagnostico = _gerador.ObterDiagnostico("TO");

            Assert.Equal(3, diagnostico.RegrasCadastradas);
            Assert.Equal(3, diagnostico.RegrasComVigenciaNaoConfirmada.Count);
            Assert.False(diagnostico.Homologado);
        }

        [Fact]
        public void ObterDiagnostico_QuandoUfRj_DeveManterRevisaoPendente()
        {
            var diagnostico = _gerador.ObterDiagnostico("RJ");

            Assert.Equal(3, diagnostico.RegrasCadastradas);
            Assert.Equal("PendenteRevisao", diagnostico.Situacao);
            Assert.Empty(diagnostico.RegrasComVigenciaNaoConfirmada);
            Assert.False(diagnostico.Homologado);
        }

        [Fact]
        public void ObterDiagnostico_QuandoUfInexistente_DeveLancarExcecao()
        {
            Assert.Throws<ArgumentException>(() => _gerador.ObterDiagnostico("XX"));
        }
    }
}
