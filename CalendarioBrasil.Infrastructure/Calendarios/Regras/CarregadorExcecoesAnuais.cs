using CalendarioBrasil.Domain.Services;
using CalendarioBrasil.Domain.ValueObjects;
using CalendarioBrasil.Infrastructure.Calendarios.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CalendarioBrasil.Infrastructure.Calendarios.Regras
{
    internal static class CarregadorExcecoesAnuais
    {
        private const string Prefixo = "CalendarioBrasil.Infrastructure.Catalogos.Excecoes.";

        private static readonly JsonSerializerOptions Opcoes = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = false,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
            MaxDepth = 12
        };

        public static IReadOnlyDictionary<(string Uf, int Ano), IReadOnlyList<ExcecaoDataEstadual>> Carregar(IReadOnlyDictionary<string, IReadOnlyList<RegraFeriadoEstadual>> catalogos, CalculadoraDatasMoveis calculadora)
        {
            ArgumentNullException.ThrowIfNull(catalogos);
            ArgumentNullException.ThrowIfNull(calculadora);

            var assembly = typeof(CarregadorExcecoesAnuais).Assembly;
            var recursos = assembly.GetManifestResourceNames().Where(nome => nome.StartsWith(Prefixo, StringComparison.Ordinal) && nome.EndsWith(".json", StringComparison.Ordinal)).OrderBy(nome => nome, StringComparer.Ordinal);
            var resultado = new Dictionary<(string Uf, int Ano), IReadOnlyList<ExcecaoDataEstadual>>();

            foreach (var recurso in recursos)
            {
                var nome = recurso[Prefixo.Length..^5];
                var partes = nome.Split('-');

                if (partes.Length != 2 || !int.TryParse(partes[1], out var ano))
                {
                    throw new InvalidDataException($"Nome de catálogo anual inválido: {recurso}.");
                }

                var uf = partes[0];
                var anoCalendario = new AnoCalendario(ano);

                if (!catalogos.TryGetValue(uf, out var regras))
                {
                    throw new InvalidDataException($"A UF {uf} não possui catálogo estadual cadastrado.");
                }

                using var stream = assembly.GetManifestResourceStream(recurso) ?? throw new InvalidDataException($"Não foi possível abrir {recurso}.");
                var arquivo = JsonSerializer.Deserialize<ExcecoesAnuaisJson>(stream, Opcoes) ?? throw new InvalidDataException($"Arquivo anual inválido: {recurso}.");

                if (arquivo.Uf != uf || arquivo.Ano != ano || arquivo.Excecoes is null)
                {
                    throw new InvalidDataException($"Os metadados de {recurso} não correspondem ao nome do arquivo.");
                }

                var pascoa = calculadora.Calcular(anoCalendario).Pascoa;
                var excecoes = new List<ExcecaoDataEstadual>();

                foreach (var item in arquivo.Excecoes)
                {
                    if (item is null)
                    {
                        throw new InvalidDataException($"Exceção nula no arquivo {recurso}.");
                    }

                    var excecao = new ExcecaoDataEstadual(uf, ano, item.Codigo, item.DataOriginal, item.DataEfetiva, item.Escopo, item.FundamentoLegal, item.FonteOficial);
                    ValidarExcecao(excecao, regras, pascoa);
                    excecoes.Add(excecao);
                }

                if (excecoes.GroupBy(item => (item.Codigo, item.Escopo)).Any(grupo => grupo.Count() > 1))
                {
                    throw new InvalidDataException($"Existem exceções duplicadas em {recurso}.");
                }

                if (!resultado.TryAdd((uf, ano), excecoes.AsReadOnly()))
                {
                    throw new InvalidDataException($"Catálogo anual duplicado: {uf}/{ano}.");
                }
            }

            return resultado;
        }

        internal static void ValidarExcecao(ExcecaoDataEstadual excecao, IReadOnlyList<RegraFeriadoEstadual> regras, DateOnly pascoa)
        {
            ArgumentNullException.ThrowIfNull(excecao);
            ArgumentNullException.ThrowIfNull(regras);

            if (excecao.Escopo is not ("Geral" or "ExecutivoEstadual" or "JudiciarioEstadual"))
            {
                throw new InvalidDataException($"Escopo inválido para {excecao.Codigo}.");
            }

            if (string.IsNullOrWhiteSpace(excecao.Codigo) || string.IsNullOrWhiteSpace(excecao.FundamentoLegal) || string.IsNullOrWhiteSpace(excecao.FonteOficial))
            {
                throw new InvalidDataException("A exceção exige código, fundamento legal e fonte oficial.");
            }

            if (excecao.DataOriginal.Year != excecao.Ano || excecao.DataEfetiva.Year != excecao.Ano)
            {
                throw new InvalidDataException($"A exceção {excecao.Codigo} deve permanecer no ano do arquivo.");
            }

            if (!Uri.TryCreate(excecao.FonteOficial, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps || !string.IsNullOrEmpty(uri.UserInfo))
            {
                throw new InvalidDataException($"Fonte oficial inválida para {excecao.Codigo}.");
            }

            var regra = regras.SingleOrDefault(item => item.Codigo == excecao.Codigo);

            if (regra is null || !regra.EstaVigente(excecao.DataOriginal) || regra.CalcularDataOriginal(excecao.Ano, pascoa) != excecao.DataOriginal)
            {
                throw new InvalidDataException($"A exceção {excecao.Codigo} não corresponde à regra original cadastrada.");
            }
        }
    }
}
