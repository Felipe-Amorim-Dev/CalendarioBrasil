using CalendarioBrasil.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Domain.Services
{
    public sealed class CalculadoraDatasRecorrentes
    {
        public DateOnly Calcular(int ano, int mes, DayOfWeek diaSemana, int ocorrencia)
        {
            _ = new AnoCalendario(ano);

            if (mes < 1 || mes > 12)
            {
                throw new ArgumentOutOfRangeException(nameof(mes));
            }

            if (!Enum.IsDefined(diaSemana))
            {
                throw new ArgumentOutOfRangeException(nameof(diaSemana));
            }

            if (ocorrencia < 1 || ocorrencia > 5)
            {
                throw new ArgumentOutOfRangeException(nameof(ocorrencia));
            }

            var primeiroDia = new DateOnly(ano, mes, 1);
            int deslocamento = ((int)diaSemana - (int)primeiroDia.DayOfWeek + 7) % 7;
            var resultado = primeiroDia.AddDays(deslocamento + (ocorrencia - 1) * 7);

            if (resultado.Month != mes)
            {
                throw new ArgumentOutOfRangeException(nameof(ocorrencia), "A ocorrência solicitada não existe no mês.");
            }

            return resultado;
        }
    }
}
