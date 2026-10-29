using CalendarioBrasil.Domain.Enums;
using CalendarioBrasil.Domain.Services;
using CalendarioBrasil.Domain.ValueObjects;
using CalendarioBrasil.Infrastructure.Catalogos;
using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Tests.Infrastructure
{
    public sealed class CatalogoEventosComplementaresTests
    {
        private readonly CatalogoEventosComplementares _catalogo = new(new CalculadoraDatasMoveis(), new CalculadoraDatasRecorrentes());

        [Fact]
        public void Gerar_QuandoAno2027_DeveRetornarTresDatasComemorativas()
        {
            var eventos = _catalogo.Gerar(new AnoCalendario(2027));

            Assert.Equal(3, eventos.Count);
            Assert.All(eventos, evento => Assert.Equal(TipoEventoCalendario.DataComemorativa, evento.Tipo));
            Assert.Contains(eventos, evento => evento.Codigo == "DIA_DAS_MAES" && evento.Data == new DateOnly(2027, 5, 9));
            Assert.Contains(eventos, evento => evento.Codigo == "DIA_DOS_PAIS" && evento.Data == new DateOnly(2027, 8, 8));
        }

        [Fact]
        public void Gerar_QuandoAno2026_DeveIncluirPontosFacultativosFederais()
        {
            var eventos = _catalogo.Gerar(new AnoCalendario(2026));
            var facultativos = eventos.Where(evento => evento.Tipo == TipoEventoCalendario.PontoFacultativo).ToArray();

            Assert.Equal(2, facultativos.Length);
            Assert.Contains(facultativos, evento => evento.Data == new DateOnly(2026, 2, 16));
            Assert.Contains(facultativos, evento => evento.Data == new DateOnly(2026, 2, 17));
            Assert.All(facultativos, evento => Assert.Equal(EsferaAplicacaoEvento.AdministracaoPublicaFederal, evento.Esfera));
        }

        [Fact]
        public void Gerar_QuandoAno2027_NaoDeveProjetarPontosFacultativosDe2026()
        {
            var eventos = _catalogo.Gerar(new AnoCalendario(2027));

            Assert.DoesNotContain(eventos, evento => evento.Tipo == TipoEventoCalendario.PontoFacultativo);
        }

        [Fact]
        public void Gerar_NaoDeveRetornarCodigosDuplicados()
        {
            var eventos = _catalogo.Gerar(new AnoCalendario(2026));

            Assert.Equal(eventos.Count, eventos.Select(evento => evento.Codigo).Distinct().Count());
        }

        [Fact]
        public void Gerar_QuandoAnoNulo_DeveLancarExcecao()
        {
            Assert.Throws<ArgumentNullException>(() => _catalogo.Gerar(null!));
        }
    }
}
