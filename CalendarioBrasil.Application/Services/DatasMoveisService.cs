using CalendarioBrasil.Application.DTOs;
using CalendarioBrasil.Application.Interfaces;
using CalendarioBrasil.Domain.Services;
using CalendarioBrasil.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Application.Services
{
    public sealed class DatasMoveisService : IDatasMoveisService
    {
        private readonly CalculadoraDatasMoveis _calculadora;

        public DatasMoveisService(CalculadoraDatasMoveis calculadora)
        {
            _calculadora = calculadora ?? throw new ArgumentNullException(nameof(calculadora));
        }

        public DatasMoveisDto ObterPorAno(int ano)
        {
            var anoCalendario = new AnoCalendario(ano);
            var datas = _calculadora.Calcular(anoCalendario);

            return new DatasMoveisDto(ano, datas.Carnaval, datas.QuartaFeiraCinzas, datas.SextaFeiraSanta, datas.Pascoa, datas.CorpusChristi, "Datas calculadas matematicamente. A classificação como feriado ou ponto facultativo depende da legislação e da localidade.");
        }
    }
}
