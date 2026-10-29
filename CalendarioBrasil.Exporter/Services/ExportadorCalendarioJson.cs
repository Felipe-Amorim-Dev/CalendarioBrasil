using CalendarioBrasil.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace CalendarioBrasil.Exporter.Services
{
    public sealed class ExportadorCalendarioJson
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };
        private readonly IEventosService _eventosService;

        public ExportadorCalendarioJson(IEventosService eventosService)
        {
            _eventosService = eventosService ?? throw new ArgumentNullException(nameof(eventosService));
        }

        public async Task<string> ExportarAsync(int ano, string uf, string diretorioSaida, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(uf);
            ArgumentException.ThrowIfNullOrWhiteSpace(diretorioSaida);

            if (!_eventosService.SuportaUf(uf))
            {
                throw new ArgumentException($"A UF {uf} não possui catálogo disponível.", nameof(uf));
            }

            var calendario = _eventosService.ObterPorAnoEUf(ano, uf);
            var diretorioAno = Path.Combine(Path.GetFullPath(diretorioSaida), calendario.Ano.ToString(System.Globalization.CultureInfo.InvariantCulture));

            Directory.CreateDirectory(diretorioAno);

            var arquivoFinal = Path.Combine(diretorioAno, $"{calendario.Uf}.json");
            var arquivoTemporario = Path.Combine(diretorioAno, $".{calendario.Uf}.{Guid.NewGuid():N}.tmp");

            try
            {
                await using (var stream = new FileStream(arquivoTemporario, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, useAsync: true))
                {
                    await JsonSerializer.SerializeAsync(stream, calendario, JsonOptions, cancellationToken);
                    await stream.FlushAsync(cancellationToken);
                }

                File.Move(arquivoTemporario, arquivoFinal, true);

                return arquivoFinal;
            }
            finally
            {
                if (File.Exists(arquivoTemporario))
                {
                    File.Delete(arquivoTemporario);
                }
            }
        }
    }
}
