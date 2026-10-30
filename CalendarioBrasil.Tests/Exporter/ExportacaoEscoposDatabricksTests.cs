using CalendarioBrasil.Application.Interfaces;
using CalendarioBrasil.Application.Services;
using CalendarioBrasil.Domain.Services;
using CalendarioBrasil.Exporter.Services;
using CalendarioBrasil.Infrastructure.Calendarios;
using CalendarioBrasil.Infrastructure.Catalogos;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CalendarioBrasil.Tests.Exporter
{
    public sealed class ExportacaoEscoposDatabricksTests
    {
        private static IEventosService CriarService()
        {
            var calculadora = new CalculadoraDatasMoveis();
            return new EventosService(new GeradorFeriadosNacionaisFixos(), new GeradorFeriadosEstaduais(calculadora), new CatalogoEventosComplementares(calculadora, new CalculadoraDatasRecorrentes()));
        }

        [Fact]
        public async Task ExportacaoPadrao_DevePreservarCaminhoAntigoEManifestoVersaoUm()
        {
            var pasta = CriarPasta();

            try
            {
                var service = CriarService();
                var manifesto = await new ExportadorLoteJson(service, new ExportadorCalendarioJson(service)).ExportarAsync(2027, 2027, ["RJ"], pasta);
                var item = Assert.Single(manifesto.Calendarios);
                Assert.Equal(1, manifesto.Versao);
                Assert.Equal("Geral", item.Escopo);
                Assert.Equal("2027/RJ.json", item.Arquivo);
                Assert.Null(item.ArquivoDatabricks);
                Assert.True(File.Exists(Path.Combine(pasta, "2027", "RJ.json")));
            }
            finally
            {
                ExcluirPasta(pasta);
            }
        }

        [Fact]
        public async Task ExportacaoMultiEscopo_DeveGerarTresCalendariosEJsonlCorrespondentes()
        {
            var pasta = CriarPasta();

            try
            {
                var service = CriarService();
                var lote = new ExportadorLoteJson(service, new ExportadorCalendarioJson(service));
                var manifesto = await lote.ExportarAsync(2026, 2026, ["AC"], pasta, ["Geral", "ExecutivoEstadual", "JudiciarioEstadual"], true);
                Assert.Equal(2, manifesto.Versao);
                Assert.Equal(3, manifesto.Calendarios.Count);
                Assert.Equal(3, manifesto.Calendarios.Select(item => item.Escopo).Distinct().Count());

                foreach (var item in manifesto.Calendarios)
                {
                    var arquivoJson = Path.Combine(pasta, item.Arquivo.Replace('/', Path.DirectorySeparatorChar));
                    var arquivoJsonl = Path.Combine(pasta, item.ArquivoDatabricks!.Replace('/', Path.DirectorySeparatorChar));
                    Assert.True(File.Exists(arquivoJson));
                    Assert.True(File.Exists(arquivoJsonl));
                    Assert.Equal(await HashAsync(arquivoJson), item.Sha256);
                    Assert.Equal(await HashAsync(arquivoJsonl), item.Sha256Databricks);

                    var linhas = await File.ReadAllLinesAsync(arquivoJsonl);
                    Assert.Equal(item.Total, linhas.Length);

                    foreach (var linha in linhas)
                    {
                        using var documento = JsonDocument.Parse(linha);
                        Assert.Equal("AC", documento.RootElement.GetProperty("uf").GetString());
                        Assert.Equal(item.Escopo, documento.RootElement.GetProperty("escopo").GetString());
                        Assert.Equal(2026, documento.RootElement.GetProperty("ano").GetInt32());
                    }
                }

                var geral = manifesto.Calendarios.Single(item => item.Escopo == "Geral");
                var judiciario = manifesto.Calendarios.Single(item => item.Escopo == "JudiciarioEstadual");
                Assert.Equal("2026/AC.json", geral.Arquivo);
                Assert.Equal("escopos/JudiciarioEstadual/2026/AC.json", judiciario.Arquivo);

                var eventoGeral = await BuscarEventoAsync(Path.Combine(pasta, geral.Arquivo.Replace('/', Path.DirectorySeparatorChar)), "AC_TRATADO_PETROPOLIS");
                var eventoJudiciario = await BuscarEventoAsync(Path.Combine(pasta, judiciario.Arquivo.Replace('/', Path.DirectorySeparatorChar)), "AC_TRATADO_PETROPOLIS");
                Assert.Equal("2026-11-17", eventoGeral.GetProperty("data").GetString());
                Assert.Equal("2026-11-19", eventoJudiciario.GetProperty("data").GetString());
            }
            finally
            {
                ExcluirPasta(pasta);
            }
        }

        [Fact]
        public async Task ExportacaoComEscoposDuplicados_DeveGerarUmArquivoPorEscopo()
        {
            var pasta = CriarPasta();

            try
            {
                var service = CriarService();
                var lote = new ExportadorLoteJson(service, new ExportadorCalendarioJson(service));
                var manifesto = await lote.ExportarAsync(2027, 2027, ["RJ", "rj"], pasta, ["geral", "Geral", "executivoestadual"]);
                Assert.Equal(2, manifesto.Calendarios.Count);
                Assert.All(manifesto.Calendarios, item => Assert.Null(item.ArquivoDatabricks));
            }
            finally
            {
                ExcluirPasta(pasta);
            }
        }

        [Fact]
        public async Task ExportacaoComEscopoInvalido_DeveRejeitarAntesDeGerarArquivos()
        {
            var pasta = CriarPasta();

            try
            {
                var service = CriarService();
                var lote = new ExportadorLoteJson(service, new ExportadorCalendarioJson(service));
                await Assert.ThrowsAsync<ArgumentException>(() => lote.ExportarAsync(2027, 2027, ["RJ"], pasta, ["1"], true));
                Assert.False(File.Exists(Path.Combine(pasta, "manifest.json")));
            }
            finally
            {
                ExcluirPasta(pasta);
            }
        }

        private static async Task<JsonElement> BuscarEventoAsync(string caminho, string codigo)
        {
            using var documento = JsonDocument.Parse(await File.ReadAllTextAsync(caminho));
            return documento.RootElement.GetProperty("eventos").EnumerateArray().Single(item => item.GetProperty("codigo").GetString() == codigo).Clone();
        }

        private static async Task<string> HashAsync(string caminho)
        {
            return Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(caminho))).ToLowerInvariant();
        }

        private static string CriarPasta()
        {
            return Path.Combine(Path.GetTempPath(), "CalendarioBrasilTests", Guid.NewGuid().ToString("N"));
        }

        private static void ExcluirPasta(string pasta)
        {
            if (Directory.Exists(pasta))
            {
                Directory.Delete(pasta, true);
            }
        }
    }
}
