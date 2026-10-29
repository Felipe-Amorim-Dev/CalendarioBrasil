using CalendarioBrasil.Domain.Entities;
using CalendarioBrasil.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Domain.Interfaces
{
    public interface ICatalogoEventosComplementares
    {
        IReadOnlyList<EventoCalendario> Gerar(AnoCalendario ano);
    }
}
