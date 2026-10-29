using CalendarioBrasil.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Application.Interfaces
{
    public interface ICalendarioService
    {
        bool SuportaUf(string? uf);
        IReadOnlyList<string> ObterUfsSuportadas();
        CalendarioFeriadosDto ObterPorAnoEUf(int ano, string uf);
    }
}
