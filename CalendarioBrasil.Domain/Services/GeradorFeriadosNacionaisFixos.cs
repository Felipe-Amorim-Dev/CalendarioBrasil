using CalendarioBrasil.Domain.Entities;
using CalendarioBrasil.Domain.Enums;
using CalendarioBrasil.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Domain.Services
{
    public sealed class GeradorFeriadosNacionaisFixos
    {
        private const string Lei662 = "Lei nº 662/1949, art. 1º, com redação da Lei nº 10.607/2002";
        private const string Lei6802 = "Lei nº 6.802/1980";
        private const string Lei14759 = "Lei nº 14.759/2023";

        public IReadOnlyList<Feriado> Gerar(AnoCalendario ano)
        {
            ArgumentNullException.ThrowIfNull(ano);

            int anoReferencia = ano.Valor;

            return
            [
                new("CONFRATERNIZACAO_UNIVERSAL", "Confraternização Universal", new DateOnly(anoReferencia, 1, 1), AbrangenciaFeriado.Nacional, null, Lei662),
            new("TIRADENTES", "Tiradentes", new DateOnly(anoReferencia, 4, 21), AbrangenciaFeriado.Nacional, null, Lei662),
            new("DIA_TRABALHO", "Dia do Trabalho", new DateOnly(anoReferencia, 5, 1), AbrangenciaFeriado.Nacional, null, Lei662),
            new("INDEPENDENCIA_BRASIL", "Independência do Brasil", new DateOnly(anoReferencia, 9, 7), AbrangenciaFeriado.Nacional, null, Lei662),
            new("NOSSA_SENHORA_APARECIDA", "Nossa Senhora Aparecida", new DateOnly(anoReferencia, 10, 12), AbrangenciaFeriado.Nacional, null, Lei6802),
            new("FINADOS", "Finados", new DateOnly(anoReferencia, 11, 2), AbrangenciaFeriado.Nacional, null, Lei662),
            new("PROCLAMACAO_REPUBLICA", "Proclamação da República", new DateOnly(anoReferencia, 11, 15), AbrangenciaFeriado.Nacional, null, Lei662),
            new("CONSCIENCIA_NEGRA", "Dia Nacional de Zumbi e da Consciência Negra", new DateOnly(anoReferencia, 11, 20), AbrangenciaFeriado.Nacional, null, Lei14759),
            new("NATAL", "Natal", new DateOnly(anoReferencia, 12, 25), AbrangenciaFeriado.Nacional, null, Lei662)
            ];
        }
    }
}
