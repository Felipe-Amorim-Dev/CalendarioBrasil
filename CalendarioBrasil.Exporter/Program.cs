using System.Globalization;
using CalendarioBrasil.Application.Interfaces;
using CalendarioBrasil.Application.Services;
using CalendarioBrasil.Domain.Interfaces;
using CalendarioBrasil.Domain.Services;
using CalendarioBrasil.Exporter.Services;
using CalendarioBrasil.Infrastructure.Calendarios;
using CalendarioBrasil.Infrastructure.Catalogos;

namespace CalendarioBrasil.Exporter
{
    internal static class Program
    {
        private static async Task<int> Main(string[] args)
        {
            if (args.Length != 6 || !string.Equals(args[0], "--ano", StringComparison.OrdinalIgnoreCase) || !string.Equals(args[2], "--uf", StringComparison.OrdinalIgnoreCase) || !string.Equals(args[4], "--saida", StringComparison.OrdinalIgnoreCase) || !int.TryParse(args[1], NumberStyles.None, CultureInfo.InvariantCulture, out int ano))
            {
                Console.Error.WriteLine("Uso: --ano 2027 --uf RJ --saida dist");
                return 2;
            }

            try
            {
                var calculadoraMoveis = new CalculadoraDatasMoveis();
                var calculadoraRecorrentes = new CalculadoraDatasRecorrentes();

                IGeradorFeriadosEstaduais[] geradoresEstaduais = [new GeradorFeriadosEstaduaisRj(calculadoraMoveis)];
                ICatalogoEventosComplementares catalogo = new CatalogoEventosComplementares(calculadoraMoveis, calculadoraRecorrentes);

                IEventosService eventosService = new EventosService(new GeradorFeriadosNacionaisFixos(), geradoresEstaduais, catalogo);
                var exportador = new ExportadorCalendarioJson(eventosService);

                var arquivo = await exportador.ExportarAsync(ano, args[3], args[5]);

                Console.WriteLine($"Arquivo gerado: {arquivo}");
                Console.WriteLine("Atenção: o calendário possui cobertura legal parcial.");

                return 0;
            }
            catch (ArgumentException ex)
            {
                Console.Error.WriteLine($"Parâmetros inválidos: {ex.Message}");
                return 2;
            }
            catch (IOException ex)
            {
                Console.Error.WriteLine($"Erro ao gravar o arquivo: {ex.Message}");
                return 1;
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.Error.WriteLine($"Acesso ao diretório negado: {ex.Message}");
                return 1;
            }
        }
    }
}