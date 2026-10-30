using CalendarioBrasil.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Application.Constants
{
    public static class ConversorEscopoCalendario
    {
        public static bool TentarConverter(string? texto, out EscopoCalendarioEstadual escopo)
        {
            escopo = EscopoCalendarioEstadual.Geral;

            if (string.IsNullOrWhiteSpace(texto))
            {
                return false;
            }

            var nome = texto.Trim();

            if (!Enum.GetNames<EscopoCalendarioEstadual>().Any(valor => string.Equals(valor, nome, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            return Enum.TryParse(nome, true, out escopo);
        }

        public static EscopoCalendarioEstadual Converter(string? texto)
        {
            if (!TentarConverter(texto, out var escopo))
            {
                throw new ArgumentException("Escopo inválido. Utilize Geral, ExecutivoEstadual ou JudiciarioEstadual.", nameof(texto));
            }

            return escopo;
        }
    }
}