using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Application.DTOs
{
    public sealed record CalendarioEventosDto(int Ano, string Uf, string Cobertura, int Total, IReadOnlyList<EventoCalendarioDto> Eventos, IReadOnlyList<string> Limitacoes)
    {
        public IReadOnlyList<AlertaCalendarioDto> Alertas { get; init; } = [];
    }
}
