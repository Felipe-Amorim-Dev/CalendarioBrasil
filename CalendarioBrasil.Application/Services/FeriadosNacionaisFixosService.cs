using CalendarioBrasil.Application.DTOs;
using CalendarioBrasil.Application.Interfaces;
using CalendarioBrasil.Domain.Services;
using CalendarioBrasil.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Application.Services
{
    public sealed class FeriadosNacionaisFixosService : IFeriadosNacionaisFixosService
    {
        private readonly GeradorFeriadosNacionaisFixos _gerador;

        public FeriadosNacionaisFixosService(GeradorFeriadosNacionaisFixos gerador)
        {
            _gerador = gerador ?? throw new ArgumentNullException(nameof(gerador));
        }

        public IReadOnlyList<FeriadoNacionalFixoDto> ObterPorAno(int ano)
        {
            var anoCalendario = new AnoCalendario(ano);
            var feriados = _gerador.Gerar(anoCalendario);

            return feriados.Select(feriado => new FeriadoNacionalFixoDto(
                feriado.Codigo,
                feriado.Nome,
                feriado.Data,
                feriado.Abrangencia.ToString(),
                feriado.FundamentoLegal
            )).ToArray();
        }
    }
}
