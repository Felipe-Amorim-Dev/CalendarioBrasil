using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Application.DTOs
{
    public sealed record AlertaCalendarioDto(string Codigo, DateOnly Data, IReadOnlyList<string> CodigosEventos, string Descricao);
}
