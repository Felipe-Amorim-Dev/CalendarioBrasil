using CalendarioBrasil.Application.Constants;
using CalendarioBrasil.Application.DTOs;
using CalendarioBrasil.Domain.ValueObjects;
using CalendarioBrasil.Exporter.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace CalendarioBrasil.Exporter.Services
{
    public sealed class ExportadorRegistrosDatabricks
    {
        private static readonly JsonSerializerOptions Opcoes = new(JsonSerializerDefaults.Web);

        public async Task<string> ExportarAsync(CalendarioEventosDto calendario, string diretorioSaida, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(calendario);
            ArgumentException.ThrowIfNullOrWhiteSpace(diretorioSaida);
            _ = new AnoCalendario(calendario.Ano);

            if (calendario.Uf.Length != 2 || calendario.Uf.Any(caractere => caractere < 'A' || caractere > 'Z'))
            {
                throw new ArgumentException("A UF do calendário é inválida.", nameof(calendario));
            }

            var escopo = ConversorEscopoCalendario.Converter(calendario.Escopo).ToString();
            var pasta = Path.Combine(Path.GetFullPath(diretorioSaida), "databricks", calendario.Ano.ToString(System.Globalization.CultureInfo.InvariantCulture), escopo);
            var arquivo = Path.Combine(pasta, $"{calendario.Uf}.jsonl");
            Directory.CreateDirectory(pasta);
            var temporario = Path.Combine(pasta, $".{Guid.NewGuid():N}.tmp");

            try
            {
                await using (var stream = new FileStream(temporario, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, true))
                {
                    await using (var escritor = new StreamWriter(stream, new UTF8Encoding(false), 1024, true))
                    {
                        foreach (var evento in calendario.Eventos)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            var linha = JsonSerializer.Serialize(EventoDatabricks.Criar(calendario, evento), Opcoes);
                            await escritor.WriteLineAsync(linha.AsMemory(), cancellationToken);
                        }

                        await escritor.FlushAsync(cancellationToken);
                    }

                    await stream.FlushAsync(cancellationToken);
                }

                File.Move(temporario, arquivo, true);
                return arquivo;
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