using CalendarioBrasil.Domain.Entities;
using CalendarioBrasil.Domain.Enums;
using CalendarioBrasil.Domain.Interfaces;
using CalendarioBrasil.Domain.Services;
using CalendarioBrasil.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Infrastructure.Calendarios
{
    public sealed class GeradorFeriadosEstaduaisRj : IGeradorFeriadosEstaduais
    {
        private static readonly DateOnly InicioVigenciaCarnaval = new(2008, 5, 15);
        private static readonly DateOnly InicioVigenciaSaoJorge = new(2008, 3, 6);
        private static readonly DateOnly InicioVigenciaCorpusChristi = new(2025, 10, 23);

        private readonly CalculadoraDatasMoveis _calculadora;

        public string Uf => "RJ";

        public GeradorFeriadosEstaduaisRj(CalculadoraDatasMoveis calculadora)
        {
            _calculadora = calculadora ?? throw new ArgumentNullException(nameof(calculadora));
        }

        public IReadOnlyList<Feriado> Gerar(AnoCalendario ano)
        {
            ArgumentNullException.ThrowIfNull(ano);

            var datasMoveis = _calculadora.Calcular(ano);
            var feriados = new List<Feriado>();

            if (datasMoveis.Carnaval >= InicioVigenciaCarnaval)
            {
                feriados.Add(new Feriado("RJ_CARNAVAL", "Terça-feira de Carnaval", datasMoveis.Carnaval, AbrangenciaFeriado.Estadual, Uf, "Lei Estadual nº 5.243/2008"));
            }

            var dataSaoJorge = new DateOnly(ano.Valor, 4, 23);

            if (dataSaoJorge >= InicioVigenciaSaoJorge)
            {
                feriados.Add(new Feriado("RJ_SAO_JORGE", "Dia de São Jorge", dataSaoJorge, AbrangenciaFeriado.Estadual, Uf, "Lei Estadual nº 5.198/2008"));
            }

            if (datasMoveis.CorpusChristi >= InicioVigenciaCorpusChristi)
            {
                feriados.Add(new Feriado("RJ_CORPUS_CHRISTI", "Corpus Christi", datasMoveis.CorpusChristi, AbrangenciaFeriado.Estadual, Uf, "Lei Estadual nº 11.002/2025"));
            }

            return feriados.OrderBy(feriado => feriado.Data).ToArray();
        }
    }
}

