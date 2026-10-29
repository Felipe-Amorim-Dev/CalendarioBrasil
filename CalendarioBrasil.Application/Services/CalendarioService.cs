using CalendarioBrasil.Application.DTOs;
using CalendarioBrasil.Application.Interfaces;
using CalendarioBrasil.Domain.Interfaces;
using CalendarioBrasil.Domain.Services;
using CalendarioBrasil.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Application.Services
{
    public sealed class CalendarioService : ICalendarioService
    {
        private readonly GeradorFeriadosNacionaisFixos _geradorNacional;
        private readonly IReadOnlyDictionary<string, IGeradorFeriadosEstaduais> _geradoresEstaduais;

        public CalendarioService(GeradorFeriadosNacionaisFixos geradorNacional, IEnumerable<IGeradorFeriadosEstaduais> geradoresEstaduais)
        {
            _geradorNacional = geradorNacional ?? throw new ArgumentNullException(nameof(geradorNacional));
            ArgumentNullException.ThrowIfNull(geradoresEstaduais);

            _geradoresEstaduais = geradoresEstaduais.ToDictionary(gerador => gerador.Uf, StringComparer.OrdinalIgnoreCase);
        }

        public bool SuportaUf(string? uf)
        {
            return !string.IsNullOrWhiteSpace(uf) && _geradoresEstaduais.ContainsKey(uf.Trim());
        }

        public IReadOnlyList<string> ObterUfsSuportadas()
        {
            return _geradoresEstaduais.Keys.OrderBy(uf => uf).ToArray();
        }

        public CalendarioFeriadosDto ObterPorAnoEUf(int ano, string uf)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(uf);

            var anoCalendario = new AnoCalendario(ano);
            var ufNormalizada = uf.Trim().ToUpperInvariant();

            if (!_geradoresEstaduais.TryGetValue(ufNormalizada, out var geradorEstadual))
            {
                throw new ArgumentException($"A UF {ufNormalizada} ainda não possui um catálogo estadual disponível.", nameof(uf));
            }

            var feriados = _geradorNacional.Gerar(anoCalendario)
                .Concat(geradorEstadual.Gerar(anoCalendario))
                .DistinctBy(feriado => (feriado.Codigo, feriado.Data, feriado.Abrangencia, feriado.Uf))
                .OrderBy(feriado => feriado.Data)
                .ThenBy(feriado => feriado.Codigo)
                .Select(feriado => new FeriadoDto(feriado.Codigo, feriado.Nome, feriado.Data, feriado.Abrangencia.ToString(), feriado.Uf, feriado.FundamentoLegal))
                .ToArray();

            string[] limitacoes =
            [
                "O calendário inclui feriados nacionais fixos e os eventos estaduais cadastrados para a UF.",
                "Ainda não inclui todos os feriados móveis, municipais ou demais eventos que possam ser aplicáveis."
            ];

            return new CalendarioFeriadosDto(anoCalendario.Valor, ufNormalizada, "Parcial", feriados.Length, feriados, limitacoes);
        }
    }
}