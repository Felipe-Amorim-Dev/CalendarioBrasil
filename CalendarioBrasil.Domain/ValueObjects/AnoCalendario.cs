using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Domain.ValueObjects
{
    public sealed record AnoCalendario
    {
        public const int AnoMinimo = 1583;
        public const int AnoMaximo = 9999;

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