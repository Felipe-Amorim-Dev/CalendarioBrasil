using CalendarioBrasil.Domain.Entities;
using CalendarioBrasil.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Domain.Interfaces
{
    public interface IGeradorFeriadosEstaduais
    {
        string Uf { get; }
        IReadOnlyList<Feriado> Gerar(AnoCalendario ano);
    }
}
