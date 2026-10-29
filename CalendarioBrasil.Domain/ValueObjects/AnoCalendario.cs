using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Domain.ValueObjects
{
    public sealed record AnoCalendario
    {
        public const int AnoMinimo = 2024;
        public const int AnoMaximo = 2100;

        public int Valor { get; }

        public AnoCalendario(int valor)
        {
            if (valor < AnoMinimo || valor > AnoMaximo)
            {
                throw new ArgumentOutOfRangeException(nameof(valor), $"O ano deve estar entre {AnoMinimo} e {AnoMaximo}.");
            }

            Valor = valor;
        }
    }
}
