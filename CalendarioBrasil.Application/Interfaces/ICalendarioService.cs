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
        CalendarioFeriadosDto ObterPorAno(int ano, IEnumerable<string>? ufs = null, string escopo = "Geral");
        CalendarioFeriadosDto ObterPorAnoEUf(int ano, string uf, string escopo = "Geral");
    }
}
