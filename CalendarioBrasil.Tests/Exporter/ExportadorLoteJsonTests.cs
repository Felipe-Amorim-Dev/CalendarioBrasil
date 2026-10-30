using System.Security.Cryptography;
using System.Text.Json;
using CalendarioBrasil.Application.Interfaces;
using CalendarioBrasil.Application.Services;
using CalendarioBrasil.Domain.Interfaces;
using CalendarioBrasil.Domain.Services;
using CalendarioBrasil.Exporter.Services;
using CalendarioBrasil.Infrastructure.Calendarios;
using CalendarioBrasil.Infrastructure.Catalogos;

namespace CalendarioBrasil.Tests.Exporter
{
    public sealed class ExportadorLoteJsonTests
    {
        private static ExportadorLoteJson CriarExportador()
        {
            var calculadora = new CalculadoraDatasMoveis();
            var geradorEstadual = new GeradorFeriadosEstaduais(calculadora);
            var catalogo = new CatalogoEventosComplementares(calculadora, new CalculadoraDatasRecorrentes());
            IEventosService service = new EventosService(new GeradorFeriadosNacionaisFixos(), geradorEstadual, catalogo);

            return new ExportadorLoteJson(service, new ExportadorCalendarioJson(service));
        }

        [Fact]
        public async Task ExportarAsync_QuandoIntervaloValido_DeveGerarArquivosEManifesto()
        {
            var diretorio = Path.Combine(Path.GetTempPath(), "CalendarioBrasilTests", Guid.NewGuid().ToString("N"));

            try
            {
                var manifesto = await CriarExportador().ExportarAsync(2027, 2028, ["RJ", "rj"], diretorio);

                Assert.Equal(2, manifesto.Calendarios.Count);
                Assert.Equal(1, manifesto.Versao);

                foreach (var calendario in manifesto.Calendarios)
                {
                    var caminho = Path.Combine(diretorio, calendario.Ano.ToString(), $"{calendario.Uf}.json");

                    Assert.True(File.Exists(caminho));
                    Assert.Equal("Parcial", calendario.Cobertura);
                    Assert.Equal(15, calendario.Total);

                    var hash = Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(caminho))).ToLowerInvariant();

                    Assert.Equal(hash, calendario.Sha256);
                }

                Assert.True(File.Exists(Path.Combine(diretorio, "manifest.json")));
                Assert.True(File.Exists(Path.Combine(diretorio, "index.html")));
                Assert.True(File.Exists(Path.Combine(diretorio, "app.js")));
                Assert.True(File.Exists(Path.Combine(diretorio, "_headers")));
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
        public async Task ExportarAsync_DeveGerarManifestoJsonValido()
        {
            var diretorio = Path.Combine(Path.GetTempPath(), "CalendarioBrasilTests", Guid.NewGuid().ToString("N"));

            try
            {
                await CriarExportador().ExportarAsync(2027, 2027, ["RJ"], diretorio);

                var conteudo = await File.ReadAllTextAsync(Path.Combine(diretorio, "manifest.json"));
                using var documento = JsonDocument.Parse(conteudo);

                Assert.Equal(1, documento.RootElement.GetProperty("versao").GetInt32());
                Assert.Single(documento.RootElement.GetProperty("calendarios").EnumerateArray());

                var calendario = documento.RootElement.GetProperty("calendarios")[0];

                Assert.Equal(2027, calendario.GetProperty("ano").GetInt32());
                Assert.Equal("RJ", calendario.GetProperty("uf").GetString());
                Assert.Equal("2027/RJ.json", calendario.GetProperty("arquivo").GetString());
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
            await Assert.ThrowsAsync<ArgumentException>(() => CriarExportador().ExportarAsync(2027, 2028, ["RJ", "XX"], "dist"));
        }

        [Fact]
        public async Task ExportarAsync_QuandoIntervaloInvertido_DeveLancarExcecao()
        {
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => CriarExportador().ExportarAsync(2028, 2027, ["RJ"], "dist"));
        }

        [Fact]
        public async Task ExportarAsync_QuandoIntervaloExcedeLimite_DeveLancarExcecao()
        {
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => CriarExportador().ExportarAsync(2027, 2047, ["RJ"], "dist"));
        }

        [Fact]
        public async Task ExportarAsync_QuandoListaUfsVazia_DeveLancarExcecao()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => CriarExportador().ExportarAsync(2027, 2027, [], "dist"));
        }
    }
}