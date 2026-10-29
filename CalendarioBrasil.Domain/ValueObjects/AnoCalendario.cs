using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Domain.ValueObjects
{
    public sealed record AnoCalendario
    {
        public int Valor { get; }

        public AnoCalendario(int valor)
        {
            if (valor < 1900 || valor > 2100)
            {
                throw new ArgumentOutOfRangeException(nameof(valor), "O ano deve estar entre 1900 e 2100.");
            }

            Valor = valor;
        }
    }
}
