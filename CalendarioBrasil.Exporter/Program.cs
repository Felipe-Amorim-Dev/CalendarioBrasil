using CalendarioBrasil.Application.Constants;
using CalendarioBrasil.Application.Interfaces;
using CalendarioBrasil.Application.Services;
using CalendarioBrasil.Domain.Interfaces;
using CalendarioBrasil.Domain.Services;
using CalendarioBrasil.Exporter.Services;
using CalendarioBrasil.Infrastructure.Calendarios;
using CalendarioBrasil.Infrastructure.Catalogos;
using System.Globalization;

namespace CalendarioBrasil.Exporter
{
    internal static class Program
    {
        private static async Task<int> Main(string[] args)
        {
            try
            {
                var parametros = LerParametros(args);
                bool individual = parametros.ContainsKey("--ano") && parametros.ContainsKey("--uf") && parametros.ContainsKey("--saida") && !parametros.ContainsKey("--ano-inicial") && !parametros.ContainsKey("--ano-final") && !parametros.ContainsKey("--ufs") && !parametros.ContainsKey("--escopos");
                bool lote = parametros.ContainsKey("--ano-inicial") && parametros.ContainsKey("--ano-final") && parametros.ContainsKey("--ufs") && parametros.ContainsKey("--saida") && !parametros.ContainsKey("--ano") && !parametros.ContainsKey("--uf") && !parametros.ContainsKey("--escopo");

                if (!individual && !lote)
                {
                    throw new ArgumentException("Informe uma exportação individual ou em lote, sem misturar os argumentos.");
                }

                bool incluirDatabricks = parametros.TryGetValue("--databricks", out var opcao) && ObterBooleano(opcao);
                var calculadoraMoveis = new CalculadoraDatasMoveis();
                IGeradorFeriadosEstaduais geradorEstadual = new GeradorFeriadosEstaduais(calculadoraMoveis);
                ICatalogoEventosComplementares catalogo = new CatalogoEventosComplementares(calculadoraMoveis, new CalculadoraDatasRecorrentes());
                IEventosService eventosService = new EventosService(new GeradorFeriadosNacionaisFixos(), geradorEstadual, catalogo);
                var exportador = new ExportadorCalendarioJson(eventosService);

                if (individual)
                {
                    int ano = ObterAno(parametros, "--ano");
                    var escopo = ConversorEscopoCalendario.Converter(parametros.GetValueOrDefault("--escopo", "Geral")).ToString();
                    var calendario = eventosService.ObterPorAnoEUf(ano, parametros["--uf"], escopo: escopo);
                    var arquivo = await exportador.ExportarAsync(calendario, parametros["--saida"]);
                    Console.WriteLine($"Arquivo gerado: {arquivo}");

                    if (incluirDatabricks)
                    {
                        var arquivoDados = await new ExportadorRegistrosDatabricks().ExportarAsync(calendario, parametros["--saida"]);
                        Console.WriteLine($"Arquivo Databricks: {arquivoDados}");
                    }
                }
                else
                {
                    int anoInicial = ObterAno(parametros, "--ano-inicial");
                    int anoFinal = ObterAno(parametros, "--ano-final");
                    IEnumerable<string> ufs = string.Equals(parametros["--ufs"], "todas", StringComparison.OrdinalIgnoreCase) ? eventosService.ObterUfsSuportadas() : parametros["--ufs"].Split(',', StringSplitOptions.TrimEntries);
                    IEnumerable<string> escopos = ObterEscopos(parametros.GetValueOrDefault("--escopos", "Geral"));
                    var exportadorLote = new ExportadorLoteJson(eventosService, exportador);
                    var manifesto = await exportadorLote.ExportarAsync(anoInicial, anoFinal, ufs, parametros["--saida"], escopos, incluirDatabricks);
                    Console.WriteLine($"Calendários gerados: {manifesto.Calendarios.Count}");
                    Console.WriteLine($"Arquivos Databricks: {manifesto.Calendarios.Count(item => item.ArquivoDatabricks is not null)}");
                    Console.WriteLine($"Manifesto: {Path.Combine(Path.GetFullPath(parametros["--saida"]), "manifest.json")}");
                }

                Console.WriteLine("Atenção: os dados ainda possuem cobertura legal parcial e não estão homologados para uso oficial.");
                return 0;
            }
            catch (ArgumentException ex)
            {
                Console.Error.WriteLine($"Parâmetros inválidos: {ex.Message}");
                Console.Error.WriteLine("Individual: --ano 2027 --uf RJ --saida dist [--escopo Geral|ExecutivoEstadual|JudiciarioEstadual] [--databricks true]");
                Console.Error.WriteLine("Lote: --ano-inicial 2027 --ano-final 2030 --ufs todas --saida dist [--escopos todos] [--databricks true]");
                return 2;
            }
            catch (IOException ex)
            {
                Console.Error.WriteLine($"Erro de gravação: {ex.Message}");
                return 1;
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.Error.WriteLine($"Acesso negado: {ex.Message}");
                return 1;
            }
        }

        private static Dictionary<string, string> LerParametros(string[] args)
        {
            string[] permitidos = ["--ano", "--uf", "--ano-inicial", "--ano-final", "--ufs", "--saida", "--escopo", "--escopos", "--databricks"];

            if (args.Length == 0 || args.Length % 2 != 0)
            {
                throw new ArgumentException("Os argumentos devem ser informados em pares de nome e valor.");
            }

            var parametros = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < args.Length; i += 2)
            {
                if (!permitidos.Contains(args[i], StringComparer.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(args[i + 1]) || !parametros.TryAdd(args[i], args[i + 1]))
                {
                    throw new ArgumentException($"Argumento inválido ou duplicado: {args[i]}");
                }
            }

            return parametros;
        }

        private static int ObterAno(IReadOnlyDictionary<string, string> parametros, string chave)
        {
            if (!int.TryParse(parametros[chave], NumberStyles.None, CultureInfo.InvariantCulture, out int ano))
            {
                throw new ArgumentException($"O argumento {chave} deve conter um ano numérico válido.");
            }

            return ano;
        }

        private static bool ObterBooleano(string texto)
        {
            if (!bool.TryParse(texto, out bool valor))
            {
                throw new ArgumentException("O argumento --databricks deve ser true ou false.");
            }

            return valor;
        }

        private static IEnumerable<string> ObterEscopos(string texto)
        {
            if (string.Equals(texto, "todos", StringComparison.OrdinalIgnoreCase))
            {
                return ["Geral", "ExecutivoEstadual", "JudiciarioEstadual"];
            }

            return texto.Split(',', StringSplitOptions.TrimEntries);
        }
    }
}