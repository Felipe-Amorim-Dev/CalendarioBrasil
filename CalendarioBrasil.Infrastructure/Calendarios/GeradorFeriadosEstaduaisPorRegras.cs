using CalendarioBrasil.Domain.Entities;
using CalendarioBrasil.Domain.Enums;
using CalendarioBrasil.Domain.Services;
using CalendarioBrasil.Domain.ValueObjects;
using CalendarioBrasil.Infrastructure.Calendarios.Regras;
using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Infrastructure.Calendarios
{
    internal sealed class GeradorFeriadosEstaduaisPorRegras
    {
        private readonly string _uf;
        private readonly CalculadoraDatasMoveis _calculadora;
        private readonly IReadOnlyList<RegraFeriadoEstadual> _regras;

        public GeradorFeriadosEstaduaisPorRegras(string uf, CalculadoraDatasMoveis calculadora, IEnumerable<RegraFeriadoEstadual> regras)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(uf);
            ArgumentNullException.ThrowIfNull(calculadora);
            ArgumentNullException.ThrowIfNull(regras);

            var ufNormalizada = uf.Trim().ToUpperInvariant();

            if (ufNormalizada.Length != 2 || ufNormalizada.Any(caractere => caractere < 'A' || caractere > 'Z'))
            {
                throw new ArgumentException("A UF deve conter duas letras.", nameof(uf));
            }

            var regrasMaterializadas = regras.ToArray();

            if (regrasMaterializadas.Length == 0 || regrasMaterializadas.GroupBy(regra => regra.Codigo).Any(grupo => grupo.Count() > 1))
            {
                throw new ArgumentException("O catálogo deve conter regras com códigos únicos.", nameof(regras));
            }

            _uf = ufNormalizada;
            _calculadora = calculadora;
            _regras = regrasMaterializadas;
        }

        public IReadOnlyList<Feriado> Gerar(AnoCalendario ano)
        {
            ArgumentNullException.ThrowIfNull(ano);

            var pascoa = _calculadora.Calcular(ano).Pascoa;
            var feriados = new List<Feriado>();

            foreach (var regra in _regras)
            {
                var data = regra.CalcularData(ano.Valor, pascoa);

                if (!regra.EstaVigente(data))
                {
                    continue;
                }

                feriados.Add(new Feriado(regra.Codigo, regra.Nome, data, AbrangenciaFeriado.Estadual, _uf, regra.FundamentoLegal));
            }

            return feriados.OrderBy(feriado => feriado.Data).ThenBy(feriado => feriado.Codigo).ToArray();
        }
    }
}
