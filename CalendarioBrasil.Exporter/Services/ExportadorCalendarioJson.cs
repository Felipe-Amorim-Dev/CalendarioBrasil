using CalendarioBrasil.Application.Constants;
using CalendarioBrasil.Application.DTOs;
using CalendarioBrasil.Application.Interfaces;
using CalendarioBrasil.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace CalendarioBrasil.Exporter.Services
{
    public sealed class ExportadorCalendarioJson
    {
        private readonly IEventosService _eventosService;

        public ExportadorCalendarioJson(IEventosService eventosService)
        {
            _eventosService = eventosService ?? throw new ArgumentNullException(nameof(eventosService));
        }

        public Task<string> ExportarAsync(int ano, string uf, string diretorioSaida, CancellationToken cancellationToken = default)
        {
            return ExportarAsync(ano, uf, diretorioSaida, "Geral", cancellationToken);
        }

        public Task<string> ExportarAsync(int ano, string uf, string diretorioSaida, string escopo, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(uf);
            var escopoValidado = ConversorEscopoCalendario.Converter(escopo).ToString();
            var calendario = _eventosService.ObterPorAnoEUf(ano, uf, escopo: escopoValidado);
            return ExportarAsync(calendario, diretorioSaida, cancellationToken);
        }

        public async Task<string> ExportarAsync(CalendarioEventosDto calendario, string diretorioSaida, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(calendario);
            ArgumentException.ThrowIfNullOrWhiteSpace(diretorioSaida);
            ArgumentException.ThrowIfNullOrWhiteSpace(calendario.Uf);
            _ = new AnoCalendario(calendario.Ano);

            if (calendario.Uf.Length != 2 || calendario.Uf.Any(caractere => caractere < 'A' || caractere > 'Z') || !_eventosService.SuportaUf(calendario.Uf))
            {
                throw new ArgumentException("A UF do calendário é inválida ou não suportada.", nameof(calendario));
            }

            var escopo = ConversorEscopoCalendario.Converter(calendario.Escopo).ToString();
            var pasta = escopo == "Geral" ? Path.Combine(Path.GetFullPath(diretorioSaida), calendario.Ano.ToString(System.Globalization.CultureInfo.InvariantCulture)) : Path.Combine(Path.GetFullPath(diretorioSaida), "escopos", escopo, calendario.Ano.ToString(System.Globalization.CultureInfo.InvariantCulture));
            var arquivo = Path.Combine(pasta, $"{calendario.Uf}.json");
            await GravadorJsonAtomico.GravarAsync(arquivo, calendario, cancellationToken);
            return arquivo;
        }
    }
}