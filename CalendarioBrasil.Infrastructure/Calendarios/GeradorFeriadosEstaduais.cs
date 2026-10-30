using System.Text.Json;
using System.Text.Json.Serialization;
using CalendarioBrasil.Domain.Entities;
using CalendarioBrasil.Domain.Enums;
using CalendarioBrasil.Domain.Interfaces;
using CalendarioBrasil.Domain.Services;
using CalendarioBrasil.Domain.ValueObjects;
using CalendarioBrasil.Infrastructure.Calendarios.Models;
using CalendarioBrasil.Infrastructure.Calendarios.Regras;
using CalendarioBrasil.Infrastructure.Calendarios.Validacoes;

namespace CalendarioBrasil.Infrastructure.Calendarios
{
    public sealed class GeradorFeriadosEstaduais : IGeradorFeriadosEstaduais
    {
        private const string PrefixoRecursos = "CalendarioBrasil.Infrastructure.Catalogos.Estaduais.";
        private readonly IReadOnlyDictionary<string, AuditoriaEstadualJson?> _auditorias;

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

            var auditorias = new Dictionary<string, AuditoriaEstadualJson?>(StringComparer.Ordinal);
            _catalogos = CarregarCatalogos(auditorias);
            _auditorias = auditorias;
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
            var ocorrencias = new List<(RegraFeriadoEstadual Regra, DateOnly Original, DateOnly Prevista)>();

            foreach (var regra in regras)
            {
                var original = regra.CalcularDataOriginal(ano.Valor, pascoa);

                if (!regra.EstaVigente(original))
                {
                    continue;
                }

                var prevista = regra.CalcularData(ano.Valor, pascoa);
                ocorrencias.Add((regra, original, prevista));
            }

            var datas = ResolvedorTransferenciasEstaduais.Resolver(ano, ocorrencias);
            var feriados = new List<Feriado>();

            foreach (var ocorrencia in ocorrencias)
            {
                var data = datas[ocorrencia.Regra.Codigo];
                feriados.Add(new Feriado(ocorrencia.Regra.Codigo, ocorrencia.Regra.Nome, data, AbrangenciaFeriado.Estadual, ufNormalizada, ocorrencia.Regra.FundamentoLegal));
            }

            return feriados.OrderBy(feriado => feriado.Data).ThenBy(feriado => feriado.Codigo).ToArray();
        }

        public DiagnosticoCatalogoEstadual ObterDiagnostico(string uf, int? ano = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(uf);

            if (ano.HasValue)
            {
                _ = new AnoCalendario(ano.Value);
            }

            var ufNormalizada = uf.Trim().ToUpperInvariant();

            if (!_catalogos.TryGetValue(ufNormalizada, out var regras))
            {
                throw new ArgumentException($"A UF {ufNormalizada} não possui catálogo disponível.", nameof(uf));
            }

            _auditorias.TryGetValue(ufNormalizada, out var auditoria);

            var vigenciasPendentes = regras.Where(regra => !regra.VigenciaInicio.HasValue).Select(regra => regra.Codigo).OrderBy(codigo => codigo, StringComparer.Ordinal).ToArray();

            bool revisaoAprovada = auditoria?.Situacao == "Aprovada";
            bool anoValidado = ano.HasValue && auditoria?.AnoInicialValidado <= ano.Value && auditoria?.AnoFinalValidado >= ano.Value;
            bool homologado = revisaoAprovada && anoValidado && regras.Count > 0 && vigenciasPendentes.Length == 0;

            var situacao = regras.Count == 0 ? "SemRegrasCadastradas" : auditoria?.Situacao switch
            {
                "EmRevisao" => "EmRevisao",
                "Aprovada" => "Aprovada",
                _ => "PendenteRevisao"
            };

            var limitacoes = new List<string>();

            if (regras.Count == 0)
            {
                limitacoes.Add("Não existem regras estaduais cadastradas. Isso não comprova ausência de feriados.");
            }

            if (vigenciasPendentes.Length > 0)
            {
                limitacoes.Add("Existem regras cuja vigência legal ainda não foi confirmada.");
            }

            if (!revisaoAprovada)
            {
                limitacoes.Add("A revisão jurídica do catálogo ainda não foi aprovada.");
            }
            else if (!anoValidado)
            {
                limitacoes.Add("O ano solicitado não está dentro do intervalo validado ou não foi informado.");
            }

            limitacoes.Add("A revisão estadual não abrange automaticamente feriados municipais ou alterações legais posteriores.");

            return new DiagnosticoCatalogoEstadual(ufNormalizada, regras.Count, situacao, homologado, vigenciasPendentes, limitacoes, ano, auditoria?.AnoInicialValidado, auditoria?.AnoFinalValidado, auditoria?.DataRevisao, auditoria?.Responsavel, auditoria?.FontesPorCodigo);
        }

        private static IReadOnlyDictionary<string, IReadOnlyList<RegraFeriadoEstadual>> CarregarCatalogos(Dictionary<string, AuditoriaEstadualJson?> auditorias)
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

                if (catalogo.Feriados is null)
                {
                    throw new InvalidDataException($"O catálogo {recurso} não possui lista de feriados válida.");
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
                            RegraFeriadoEstadual.Fixa(item.Codigo, item.Nome, item.Mes.Value, item.Dia.Value, item.VigenciaInicio, item.FundamentoLegal, item.VigenciaFim, item.InicioEvidencia),

                        "RelativaPascoa" when item.DeslocamentoPascoa.HasValue && !item.Mes.HasValue && !item.Dia.HasValue =>
                            RegraFeriadoEstadual.RelativaPascoa(item.Codigo, item.Nome, item.DeslocamentoPascoa.Value, item.VigenciaInicio, item.FundamentoLegal, item.VigenciaFim, item.InicioEvidencia),

                        "DataFixaTransferidaDomingo" when item.Mes.HasValue && item.Dia.HasValue && !item.DeslocamentoPascoa.HasValue =>
                            RegraFeriadoEstadual.FixaTransferidaDomingo(item.Codigo, item.Nome, item.Mes.Value, item.Dia.Value, item.VigenciaInicio, item.FundamentoLegal, item.VigenciaFim, item.InicioEvidencia),

                        "DataFixaTransferidaSexta" when item.Mes.HasValue && item.Dia.HasValue && !item.DeslocamentoPascoa.HasValue =>
                            RegraFeriadoEstadual.FixaTransferidaSexta(item.Codigo, item.Nome, item.Mes.Value, item.Dia.Value, item.VigenciaInicio, item.FundamentoLegal, item.VigenciaFim, item.InicioEvidencia),

                        _ => throw new InvalidDataException($"Regra inválida no catálogo {recurso}: {item.Codigo}.")
                    };

                    regras.Add(regra);
                    
                }

                if (regras.GroupBy(regra => regra.Codigo, StringComparer.Ordinal).Any(grupo => grupo.Count() > 1))
                {
                    throw new InvalidDataException($"Existem códigos duplicados no catálogo {recurso}.");
                }

                ValidadorAuditoriaEstadual.Validar(catalogo.Auditoria, regras, ufArquivo);

                if (!catalogos.TryAdd(ufArquivo, regras.AsReadOnly()))
                {
                    throw new InvalidDataException($"Catálogo duplicado para a UF {ufArquivo}.");
                }

                auditorias.Add(ufArquivo, catalogo.Auditoria);
            }

            return catalogos;
        }
    }
}