using CalendarioBrasil.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Application.Interfaces
{
    public interface IEventosService
    {
        bool SuportaUf(string? uf);
        IReadOnlyList<string> ObterUfsSuportadas();
        CalendarioEventosDto ObterPorAnoEUf(int ano, string uf, bool incluirPontosFacultativosFederais = false);
    }
}
