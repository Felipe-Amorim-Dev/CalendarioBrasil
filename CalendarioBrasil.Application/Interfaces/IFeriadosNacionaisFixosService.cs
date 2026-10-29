using CalendarioBrasil.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Application.Interfaces
{
    public interface IFeriadosNacionaisFixosService
    {
        IReadOnlyList<FeriadoNacionalFixoDto> ObterPorAno(int ano);
    }
}
