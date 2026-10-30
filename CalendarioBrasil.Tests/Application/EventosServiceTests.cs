using CalendarioBrasil.Application.Services;
using CalendarioBrasil.Domain.Interfaces;
using CalendarioBrasil.Domain.Services;
using CalendarioBrasil.Infrastructure.Calendarios;
using CalendarioBrasil.Infrastructure.Catalogos;
using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Tests.Application
{
    public sealed class EventosServiceTests
    {
        private readonly EventosService _service = CriarService();

        private static EventosService CriarService()
        {
            var calculadoraMoveis = new CalculadoraDatasMoveis();
            var geradorEstadual = new GeradorFeriadosEstaduais(calculadoraMoveis);
            var catalogo = new CatalogoEventosComplementares(calculadoraMoveis, new CalculadoraDatasRecorrentes());

            return new EventosService(new GeradorFeriadosNacionaisFixos(), geradorEstadual, catalogo);
        }

        [Fact]
        public void ObterPorAnoEUf_QuandoAno2027_DeveRetornarQuinzeEventos()
        {
            var resultado = _service.ObterPorAnoEUf(2027, "RJ");

            Assert.Equal(15, resultado.Total);
            Assert.Equal(12, resultado.Eventos.Count(evento => evento.Tipo == "Feriado"));
            Assert.Equal(3, resultado.Eventos.Count(evento => evento.Tipo == "DataComemorativa"));
        }

        [Fact]
        public void ObterPorAnoEUf_DeveManterCoberturaParcial()
        {
            var resultado = _service.ObterPorAnoEUf(2027, "RJ");

            Assert.Equal("Parcial", resultado.Cobertura);
            Assert.NotEmpty(resultado.Limitacoes);
        }

        [Fact]
        public void ObterPorAnoEUf_QuandoAno2026_NaoDeveIncluirPontosFacultativosPorPadrao()
        {
            var resultado = _service.ObterPorAnoEUf(2026, "RJ");

            Assert.Equal(15, resultado.Total);
            Assert.DoesNotContain(resultado.Eventos, evento => evento.Tipo == "PontoFacultativo");
        }

        [Fact]
        public void ObterPorAnoEUf_QuandoSolicitado_DeveIncluirPontosFacultativosFederais()
        {
            var resultado = _service.ObterPorAnoEUf(2026, "RJ", true);

            Assert.Equal(17, resultado.Total);
            Assert.Equal(2, resultado.Eventos.Count(evento => evento.Tipo == "PontoFacultativo"));
            Assert.All(resultado.Eventos.Where(evento => evento.Tipo == "PontoFacultativo"), evento => Assert.Equal("AdministracaoPublicaFederal", evento.Esfera));
        }

        [Fact]
        public void ObterPorAnoEUf_DevePreservarDiferentesClassificacoesNaMesmaData()
        {
            var resultado = _service.ObterPorAnoEUf(2026, "RJ", true);
            var carnaval = resultado.Eventos.Where(evento => evento.Data == new DateOnly(2026, 2, 17)).ToArray();

            Assert.Equal(2, carnaval.Length);
            Assert.Contains(carnaval, evento => evento.Tipo == "Feriado" && evento.Esfera == "Estadual");
            Assert.Contains(carnaval, evento => evento.Tipo == "PontoFacultativo" && evento.Esfera == "AdministracaoPublicaFederal");
        }

        [Fact]
        public void ObterPorAnoEUf_QuandoAno2027_NaoDeveProjetarPontosFacultativosDe2026()
        {
            var resultado = _service.ObterPorAnoEUf(2027, "RJ", true);

            Assert.DoesNotContain(resultado.Eventos, evento => evento.Tipo == "PontoFacultativo");
        }

        [Fact]
        public void ObterPorAnoEUf_DeveRetornarEventosOrdenados()
        {
            var resultado = _service.ObterPorAnoEUf(2027, "RJ");
            var datas = resultado.Eventos.Select(evento => evento.Data).ToArray();

            Assert.True(datas.SequenceEqual(datas.OrderBy(data => data)));
        }

        [Fact]
        public void ObterPorAnoEUf_NaoDeveRetornarOcorrenciasDuplicadas()
        {
            var resultado = _service.ObterPorAnoEUf(2026, "RJ", true);
            var chaves = resultado.Eventos.Select(evento => (evento.Codigo, evento.Data, evento.Tipo, evento.Esfera, evento.Uf, evento.CodigoMunicipioIbge)).ToArray();

            Assert.Equal(chaves.Length, chaves.Distinct().Count());
        }

        [Fact]
        public void ObterPorAnoEUf_DeveNormalizarUf()
        {
            var resultado = _service.ObterPorAnoEUf(2027, "rj");

            Assert.Equal("RJ", resultado.Uf);
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
