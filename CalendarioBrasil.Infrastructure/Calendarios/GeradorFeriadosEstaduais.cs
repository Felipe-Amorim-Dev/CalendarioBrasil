using System.Text.Json;
using System.Text.Json.Serialization;
using CalendarioBrasil.Domain.Entities;
using CalendarioBrasil.Domain.Enums;
using CalendarioBrasil.Domain.Interfaces;
using CalendarioBrasil.Domain.Services;
using CalendarioBrasil.Domain.ValueObjects;
using CalendarioBrasil.Infrastructure.Calendarios.Models;
using CalendarioBrasil.Infrastructure.Calendarios.Regras;

namespace CalendarioBrasil.Infrastructure.Calendarios
{
    public sealed class GeradorFeriadosEstaduais : IGeradorFeriadosEstaduais
    {
        private const string PrefixoRecursos = "CalendarioBrasil.Infrastructure.Catalogos.Estaduais.";

        private static readonly HashSet<string> UfsBrasileiras = new(StringComparer.Ordinal)
        {
            "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA", "MT", "MS", "MG",
            "PA", "PB", "PR", "PE", "PI", "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO"
        };

        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = false,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
            MaxDepth = 16
        };

        private readonly CalculadoraDatasMoveis _calculadora;
        private readonly IReadOnlyDictionary<string, IReadOnlyList<RegraFeriadoEstadual>> _catalogos;

        public GeradorFeriadosEstaduais(CalculadoraDatasMoveis calculadora)
        {
            _calculadora = calculadora ?? throw new ArgumentNullException(nameof(calculadora));
            _catalogos = CarregarCatalogos();
        }

        public IReadOnlyList<string> ObterUfsDisponiveis()
        {
            return _catalogos.Keys.OrderBy(uf => uf, StringComparer.Ordinal).ToArray();
        }

        public IReadOnlyList<Feriado> Gerar(AnoCalendario ano, string uf)
        {
            ArgumentNullException.ThrowIfNull(ano);
            ArgumentException.ThrowIfNullOrWhiteSpace(uf);

            var ufNormalizada = uf.Trim().ToUpperInvariant();

            if (!_catalogos.TryGetValue(ufNormalizada, out var regras))
            {
                throw new ArgumentException($"A UF {ufNormalizada} não possui catálogo estadual cadastrado.", nameof(uf));
            }

            var pascoa = _calculadora.Calcular(ano).Pascoa;
            var feriados = new List<Feriado>();

            foreach (var regra in regras)
            {
                var data = regra.CalcularData(ano.Valor, pascoa);

                if (!regra.EstaVigente(data))
                {
                    continue;
                }

                feriados.Add(new Feriado(regra.Codigo, regra.Nome, data, AbrangenciaFeriado.Estadual, ufNormalizada, regra.FundamentoLegal));
            }

            return feriados.OrderBy(feriado => feriado.Data).ThenBy(feriado => feriado.Codigo).ToArray();
        }

        private static IReadOnlyDictionary<string, IReadOnlyList<RegraFeriadoEstadual>> CarregarCatalogos()
        {
            var assembly = typeof(GeradorFeriadosEstaduais).Assembly;
            var recursos = assembly.GetManifestResourceNames()
                .Where(nome => nome.StartsWith(PrefixoRecursos, StringComparison.Ordinal) && nome.EndsWith(".json", StringComparison.Ordinal))
                .OrderBy(nome => nome, StringComparer.Ordinal)
                .ToArray();

            if (recursos.Length == 0)
            {
                throw new InvalidDataException("Nenhum catálogo estadual JSON foi incorporado à Infrastructure.");
            }

            var catalogos = new Dictionary<string, IReadOnlyList<RegraFeriadoEstadual>>(StringComparer.OrdinalIgnoreCase);

            foreach (var recurso in recursos)
            {
                var ufArquivo = recurso[PrefixoRecursos.Length..^5];

                if (!UfsBrasileiras.Contains(ufArquivo))
                {
                    throw new InvalidDataException($"O recurso {recurso} não corresponde a uma UF brasileira válida.");
                }

                using var stream = assembly.GetManifestResourceStream(recurso) ?? throw new InvalidDataException($"Não foi possível abrir {recurso}.");
                var catalogo = JsonSerializer.Deserialize<CatalogoEstadualJson>(stream, JsonOptions) ?? throw new InvalidDataException($"Catálogo vazio: {recurso}.");

                if (!string.Equals(catalogo.Uf, ufArquivo, StringComparison.Ordinal))
                {
                    throw new InvalidDataException($"A UF do catálogo {recurso} não corresponde ao nome do arquivo.");
                }

                if (catalogo.Feriados is null || catalogo.Feriados.Count == 0)
                {
                    throw new InvalidDataException($"O catálogo {recurso} não possui feriados cadastrados.");
                }

                var regras = new List<RegraFeriadoEstadual>();

                foreach (var item in catalogo.Feriados)
                {
                    if (item is null || string.IsNullOrWhiteSpace(item.Codigo) || !item.Codigo.StartsWith($"{ufArquivo}_", StringComparison.Ordinal))
                    {
                        throw new InvalidDataException($"Código de feriado inválido no catálogo {recurso}.");
                    }

                    RegraFeriadoEstadual regra = item.TipoRegra switch
                    {
                        "DataFixa" when item.Mes.HasValue && item.Dia.HasValue && !item.DeslocamentoPascoa.HasValue =>
                            RegraFeriadoEstadual.Fixa(item.Codigo, item.Nome, item.Mes.Value, item.Dia.Value, item.VigenciaInicio, item.FundamentoLegal, item.VigenciaFim),

                        "RelativaPascoa" when item.DeslocamentoPascoa.HasValue && !item.Mes.HasValue && !item.Dia.HasValue =>
                            RegraFeriadoEstadual.RelativaPascoa(item.Codigo, item.Nome, item.DeslocamentoPascoa.Value, item.VigenciaInicio, item.FundamentoLegal, item.VigenciaFim),

                        _ => throw new InvalidDataException($"Regra inválida no catálogo {recurso}: {item.Codigo}.")
                    };

                    regras.Add(regra);
                }

                if (regras.GroupBy(regra => regra.Codigo, StringComparer.Ordinal).Any(grupo => grupo.Count() > 1))
                {
                    throw new InvalidDataException($"Existem códigos duplicados no catálogo {recurso}.");
                }

                if (!catalogos.TryAdd(ufArquivo, regras.AsReadOnly()))
                {
                    throw new InvalidDataException($"Catálogo duplicado para a UF {ufArquivo}.");
                }
            }

            return catalogos;
        }
    }
}