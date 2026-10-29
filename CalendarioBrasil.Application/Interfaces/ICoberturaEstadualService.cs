using CalendarioBrasil.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Application.Interfaces
{
    public interface ICoberturaEstadualService
    {
        bool SuportaUf(string? uf);
        CoberturaEstadualDto ObterPorUf(string uf);
    }
}
