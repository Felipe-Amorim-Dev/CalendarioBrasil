using System.Text.Json;
using CalendarioBrasil.Application.DTOs;
using CalendarioBrasil.Application.Interfaces;
using CalendarioBrasil.Application.Services;
using CalendarioBrasil.Domain.Interfaces;
using CalendarioBrasil.Domain.Services;
using CalendarioBrasil.Exporter.Services;
using CalendarioBrasil.Infrastructure.Calendarios;
using CalendarioBrasil.Infrastructure.Catalogos;

namespace CalendarioBrasil.Tests.Exporter
{
    public sealed class ExportadorCalendarioJsonTests
    {
        private static IEventosService CriarService()
        {
            var calculadoraMoveis = new CalculadoraDatasMoveis();
            IGeradorFeriadosEstaduais[] geradores = [new GeradorFeriadosEstaduaisRj(calculadoraMoveis)];
            var catalogo = new CatalogoEventosComplementares(calculadoraMoveis, new CalculadoraDatasRecorrentes());

            return new EventosService(new GeradorFeriadosNacionaisFixos(), geradores, catalogo);
        }

        [Fact]
        public async Task ExportarAsync_QuandoAno2027_DeveGerarJsonCorreto()
        {
            var diretorio = Path.Combine(Path.GetTempPath(), "CalendarioBrasilTests", Guid.NewGuid().ToString("N"));

            try
            {
                var service = CriarService();
                var exportador = new ExportadorCalendarioJson(service);
                var caminho = await exportador.ExportarAsync(2027, "rj", diretorio);

                Assert.True(File.Exists(caminho));
                Assert.Equal(Path.Combine(diretorio, "2027", "RJ.json"), caminho);

                var json = await File.ReadAllTextAsync(caminho);
                var resultado = JsonSerializer.Deserialize<CalendarioEventosDto>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));
                var esperado = service.ObterPorAnoEUf(2027, "RJ");

                Assert.NotNull(resultado);
                Assert.Equal(esperado.Total, resultado.Total);
                Assert.Equal("Parcial", resultado.Cobertura);
                Assert.Equal(esperado.Eventos.Select(evento => evento.Codigo), resultado.Eventos.Select(evento => evento.Codigo));
                Assert.Contains("\"data\": \"2027-02-09\"", json);
            }
            finally
            {
                if (Directory.Exists(diretorio))
                {
                    Directory.Delete(diretorio, true);
                }
            }
        }

        [Fact]
        public async Task ExportarAsync_QuandoExecutadoNovamente_DeveSubstituirArquivoSemAlterarConteudo()
        {
            var diretorio = Path.Combine(Path.GetTempPath(), "CalendarioBrasilTests", Guid.NewGuid().ToString("N"));

            try
            {
                var exportador = new ExportadorCalendarioJson(CriarService());

                var caminho = await exportador.ExportarAsync(2027, "RJ", diretorio);
                var primeiroConteudo = await File.ReadAllBytesAsync(caminho);

                var segundoCaminho = await exportador.ExportarAsync(2027, "RJ", diretorio);
                var segundoConteudo = await File.ReadAllBytesAsync(segundoCaminho);

                Assert.Equal(caminho, segundoCaminho);
                Assert.Equal(primeiroConteudo, segundoConteudo);
                Assert.Single(Directory.GetFiles(Path.Combine(diretorio, "2027")));
            }
            finally
            {
                if (Directory.Exists(diretorio))
                {
                    Directory.Delete(diretorio, true);
                }
            }
        }

        [Fact]
        public async Task ExportarAsync_QuandoUfNaoSuportada_DeveLancarExcecao()
        {
            var exportador = new ExportadorCalendarioJson(CriarService());

            await Assert.ThrowsAsync<ArgumentException>(() => exportador.ExportarAsync(2027, "SP", "dist"));
        }

        [Fact]
        public async Task ExportarAsync_QuandoAnoInvalido_DeveLancarExcecao()
        {
            var exportador = new ExportadorCalendarioJson(CriarService());

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => exportador.ExportarAsync(2023, "RJ", "dist"));
        }
    }
}