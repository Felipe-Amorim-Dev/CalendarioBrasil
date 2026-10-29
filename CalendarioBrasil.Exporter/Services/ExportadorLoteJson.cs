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
        private readonly TimeProvider _relogio;

        public ExportadorLoteJson(IEventosService eventosService, ExportadorCalendarioJson exportador, TimeProvider? relogio = null)
        {
            _eventosService = eventosService ?? throw new ArgumentNullException(nameof(eventosService));
            _exportador = exportador ?? throw new ArgumentNullException(nameof(exportador));
            _relogio = relogio ?? TimeProvider.System;
        }

        public async Task<ManifestoCalendarios> ExportarAsync(int anoInicial, int anoFinal, IEnumerable<string> ufs, string diretorioSaida, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(ufs);
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

            var diretorio = Path.GetFullPath(diretorioSaida);
            var arquivos = new List<CalendarioPublicado>();

            for (int ano = anoInicial; ano <= anoFinal; ano++)
            {
                foreach (var uf in ufsNormalizadas)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var calendario = _eventosService.ObterPorAnoEUf(ano, uf);
                    var caminho = await _exportador.ExportarAsync(calendario, diretorio, cancellationToken);
                    var conteudo = await File.ReadAllBytesAsync(caminho, cancellationToken);
                    var sha256 = Convert.ToHexString(SHA256.HashData(conteudo)).ToLowerInvariant();

                    arquivos.Add(new CalendarioPublicado(ano, uf, $"{ano}/{uf}.json", calendario.Cobertura, calendario.Total, sha256));
                }
            }

            foreach (var recurso in new[] { "index.html", "app.js", "_headers" })
            {
                await CopiarRecursoAsync(recurso, diretorio, cancellationToken);
            }

            var manifesto = new ManifestoCalendarios(1, _relogio.GetUtcNow(), arquivos);

            await GravadorJsonAtomico.GravarAsync(Path.Combine(diretorio, "manifest.json"), manifesto, cancellationToken);

            return manifesto;
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
