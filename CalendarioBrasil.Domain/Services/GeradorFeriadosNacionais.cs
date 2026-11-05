using CalendarioBrasil.Domain.Entities;
using CalendarioBrasil.Domain.Enums;
using CalendarioBrasil.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Domain.Services
{
    public sealed class GeradorFeriadosNacionais
    {
        private readonly GeradorFeriadosNacionaisFixos _geradorFixos;
        private readonly CalculadoraDatasMoveis _calculadoraMoveis;

        public GeradorFeriadosNacionais(GeradorFeriadosNacionaisFixos geradorFixos, CalculadoraDatasMoveis calculadoraMoveis)
        {
            _geradorFixos = geradorFixos ?? throw new ArgumentNullException(nameof(geradorFixos));
            _calculadoraMoveis = calculadoraMoveis ?? throw new ArgumentNullException(nameof(calculadoraMoveis));
        }

        public IReadOnlyList<Feriado> Gerar(AnoCalendario ano)
        {
            ArgumentNullException.ThrowIfNull(ano);

            var feriados = _geradorFixos.Gerar(ano).ToList();
            var sextaFeiraSanta = _calculadoraMoveis.Calcular(ano).SextaFeiraSanta;

            feriados.Add(new Feriado(
                "PAIXAO_CRISTO",
                "Paixão de Cristo (Sexta-feira Santa)",
                sextaFeiraSanta,
                AbrangenciaFeriado.Nacional,
                null,
                "Lei nº 9.093/1995, art. 2º (feriado religioso condicionado à lei municipal); Portaria MGI nº 11.460/2025 (referência do calendário federal de 2026)"));

            return feriados.OrderBy(feriado => feriado.Data).ThenBy(feriado => feriado.Codigo).ToArray();
        }
    }
}
