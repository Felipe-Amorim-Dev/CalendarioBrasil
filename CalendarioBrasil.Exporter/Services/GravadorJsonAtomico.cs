using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace CalendarioBrasil.Exporter.Services
{
    internal static class GravadorJsonAtomico
    {
        private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web) { WriteIndented = true };

        public static async Task GravarAsync<T>(string caminho, T dados, CancellationToken cancellationToken = default)
        {
            var caminhoCompleto = Path.GetFullPath(caminho);
            var diretorio = Path.GetDirectoryName(caminhoCompleto) ?? throw new InvalidOperationException("Diretório inválido.");

            Directory.CreateDirectory(diretorio);

            var temporario = Path.Combine(diretorio, $".{Guid.NewGuid():N}.tmp");

            try
            {
                await using (var stream = new FileStream(temporario, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, true))
                {
                    await JsonSerializer.SerializeAsync(stream, dados, Options, cancellationToken);
                    await stream.FlushAsync(cancellationToken);
                }

                File.Move(temporario, caminhoCompleto, true);
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
