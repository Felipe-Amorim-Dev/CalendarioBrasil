using CalendarioBrasil.Application.DTOs;
using CalendarioBrasil.Application.Interfaces;
using CalendarioBrasil.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Application.Services
{
    public sealed class CoberturaEstadualService : ICoberturaEstadualService
    {
        private readonly IGeradorFeriadosEstaduais _gerador;

        public CoberturaEstadualService(IGeradorFeriadosEstaduais gerador)
        {
            _gerador = gerador ?? throw new ArgumentNullException(nameof(gerador));
        }

        public bool SuportaUf(string? uf)
        {
            return !string.IsNullOrWhiteSpace(uf) && _gerador.ObterUfsDisponiveis().Contains(uf.Trim(), StringComparer.OrdinalIgnoreCase);
        }

        public CoberturaEstadualDto ObterPorUf(string uf)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(uf);

            var diagnostico = _gerador.ObterDiagnostico(uf);

            return new CoberturaEstadualDto(diagnostico.Uf, diagnostico.RegrasCadastradas, diagnostico.Situacao, diagnostico.Homologado, diagnostico.RegrasComVigenciaNaoConfirmada, diagnostico.Limitacoes);
        }
    }
}
