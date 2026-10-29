using CalendarioBrasil.Domain.Entities;
using CalendarioBrasil.Domain.Interfaces;
using CalendarioBrasil.Domain.Services;
using CalendarioBrasil.Domain.ValueObjects;
using CalendarioBrasil.Infrastructure.Calendarios.Regras;

namespace CalendarioBrasil.Infrastructure.Calendarios
{
    public sealed class GeradorFeriadosEstaduaisRj : IGeradorFeriadosEstaduais
    {
        private readonly GeradorFeriadosEstaduaisPorRegras _gerador;

        public string Uf => "RJ";

        public GeradorFeriadosEstaduaisRj(CalculadoraDatasMoveis calculadora)
        {
            RegraFeriadoEstadual[] regras =
            [
                RegraFeriadoEstadual.RelativaPascoa("RJ_CARNAVAL", "Terça-feira de Carnaval", -47, new DateOnly(2008, 5, 15), "Lei Estadual nº 5.243/2008"),
                RegraFeriadoEstadual.Fixa("RJ_SAO_JORGE", "Dia de São Jorge", 4, 23, new DateOnly(2008, 3, 6), "Lei Estadual nº 5.198/2008"),
                RegraFeriadoEstadual.RelativaPascoa("RJ_CORPUS_CHRISTI", "Corpus Christi", 60, new DateOnly(2025, 10, 23), "Lei Estadual nº 11.002/2025")
            ];

            _gerador = new GeradorFeriadosEstaduaisPorRegras(Uf, calculadora, regras);
        }

        public IReadOnlyList<Feriado> Gerar(AnoCalendario ano)
        {
            return _gerador.Gerar(ano);
        }
    }
}