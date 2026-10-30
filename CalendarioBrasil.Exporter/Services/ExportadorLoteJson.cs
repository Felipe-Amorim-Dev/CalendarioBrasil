using CalendarioBrasil.Application.Constants;
using CalendarioBrasil.Application.Interfaces;
using CalendarioBrasil.Domain.ValueObjects;
using CalendarioBrasil.Exporter.Models;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace CalendarioBrasil.Exporter.Services
{
    public sealed class ExportadorLoteJson
    {
        private const int MaximoAnosPorLote = 20;
        private readonly IEventosService _eventosService;
        private readonly ExportadorCalendarioJson _exportador;
        private readonly ExportadorRegistrosDatabricks _exportadorDatabricks;
        private readonly TimeProvider _relogio;

        public ExportadorLoteJson(IEventosService eventosService, ExportadorCalendarioJson exportador, TimeProvider? relogio = null)
        {
            _eventosService = eventosService ?? throw new ArgumentNullException(nameof(eventosService));
            _exportador = exportador ?? throw new ArgumentNullException(nameof(exportador));
            _exportadorDatabricks = new ExportadorRegistrosDatabricks();
            _relogio = relogio ?? TimeProvider.System;
        }

        public Task<ManifestoCalendarios> ExportarAsync(int anoInicial, int anoFinal, IEnumerable<string> ufs, string diretorioSaida, CancellationToken cancellationToken = default)
        {
            return ExportarAsync(anoInicial, anoFinal, ufs, diretorioSaida, ["Geral"], false, cancellationToken);
        }

        public async Task<ManifestoCalendarios> ExportarAsync(int anoInicial, int anoFinal, IEnumerable<string> ufs, string diretorioSaida, IEnumerable<string> escopos, bool incluirDatabricks = false, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(ufs);
            ArgumentNullException.ThrowIfNull(escopos);
            ArgumentException.ThrowIfNullOrWhiteSpace(diretorioSaida);
            _ = new AnoCalendario(anoInicial);
            _ = new AnoCalendario(anoFinal);

            if (anoFinal < anoInicial || anoFinal - anoInicial + 1 > MaximoAnosPorLote)
            {
                throw new ArgumentOutOfRangeException(nameof(anoFinal), $"O intervalo deve conter de 1 a {MaximoAnosPorLote} anos.");
            }

            var listaUfs = ufs.ToArray();

            if (listaUfs.Length == 0 || listaUfs.Any(string.IsNullOrWhiteSpace))
            {
                throw new ArgumentException("Informe pelo menos uma UF.", nameof(ufs));
            }

            var ufsNormalizadas = listaUfs.Select(uf => uf.Trim().ToUpperInvariant()).Distinct(StringComparer.Ordinal).OrderBy(uf => uf, StringComparer.Ordinal).ToArray();

            foreach (var uf in ufsNormalizadas)
            {
                if (!_eventosService.SuportaUf(uf))
                {
                    throw new ArgumentException($"A UF {uf} ainda não possui catálogo disponível.", nameof(ufs));
                }
            }

            var nomesEscopos = escopos.ToArray();

            if (nomesEscopos.Length == 0)
            {
                throw new ArgumentException("Informe pelo menos um escopo.", nameof(escopos));
            }

            var escoposNormalizados = nomesEscopos.Select(nome => ConversorEscopoCalendario.Converter(nome)).Distinct().OrderBy(escopo => (int)escopo).Select(escopo => escopo.ToString()).ToArray();
            var diretorio = Path.GetFullPath(diretorioSaida);
            var arquivos = new List<CalendarioPublicado>();

            for (int ano = anoInicial; ano <= anoFinal; ano++)
            {
                foreach (var escopo in escoposNormalizados)
                {
                    foreach (var uf in ufsNormalizadas)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var calendario = _eventosService.ObterPorAnoEUf(ano, uf, escopo: escopo);
                        var caminho = await _exportador.ExportarAsync(calendario, diretorio, cancellationToken);
                        var hash = await CalcularHashAsync(caminho, cancellationToken);
                        string? arquivoDatabricks = null;
                        string? hashDatabricks = null;

                        if (incluirDatabricks)
                        {
                            var caminhoDatabricks = await _exportadorDatabricks.ExportarAsync(calendario, diretorio, cancellationToken);
                            arquivoDatabricks = Path.GetRelativePath(diretorio, caminhoDatabricks).Replace('\\', '/');
                            hashDatabricks = await CalcularHashAsync(caminhoDatabricks, cancellationToken);
                        }

                        var relativo = Path.GetRelativePath(diretorio, caminho).Replace('\\', '/');
                        arquivos.Add(new CalendarioPublicado(ano, uf, relativo, calendario.Cobertura, calendario.Total, hash, escopo, arquivoDatabricks, hashDatabricks));
                    }
                }
            }

            foreach (var recurso in new[] { "index.html", "app.js", "_headers" })
            {
                await CopiarRecursoAsync(recurso, diretorio, cancellationToken);
            }

            var versao = escoposNormalizados.Length == 1 && escoposNormalizados[0] == "Geral" && !incluirDatabricks ? 1 : 2;
            var manifesto = new ManifestoCalendarios(versao, _relogio.GetUtcNow(), arquivos);
            await GravadorJsonAtomico.GravarAsync(Path.Combine(diretorio, "manifest.json"), manifesto, cancellationToken);
            return manifesto;
        }

        private static async Task<string> CalcularHashAsync(string arquivo, CancellationToken cancellationToken)
        {
            await using var stream = new FileStream(arquivo, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, true);
            var hash = await SHA256.HashDataAsync(stream, cancellationToken);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        private static async Task CopiarRecursoAsync(string nome, string diretorio, CancellationToken cancellationToken)
        {
            var assembly = typeof(ExportadorLoteJson).Assembly;
            var nomeRecurso = $"CalendarioBrasil.Exporter.Site.{nome}";
            using var origem = assembly.GetManifestResourceStream(nomeRecurso) ?? throw new InvalidOperationException($"Recurso não encontrado: {nomeRecurso}");
            Directory.CreateDirectory(diretorio);
            var destino = Path.Combine(diretorio, nome);
            var temporario = Path.Combine(diretorio, $".{Guid.NewGuid():N}.tmp");

            try
            {
                await using (var stream = new FileStream(temporario, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, true))
                {
                    await origem.CopyToAsync(stream, cancellationToken);
                    await stream.FlushAsync(cancellationToken);
                }

                File.Move(temporario, destino, true);
            }
            finally
            {
                if (File.Exists(temporario))
                {
                    File.Delete(temporario);
                }
            }
        }
    }
}
