using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Infrastructure.Calendarios.Regras
{
    internal sealed record RegraFeriadoEstadual(string Codigo, string Nome, int? Mes, int? Dia, int? DeslocamentoPascoa, DateOnly VigenciaInicio, DateOnly? VigenciaFim, string FundamentoLegal)
    {
        public bool TransferirDomingoSeguinte { get; init; }

        public static RegraFeriadoEstadual Fixa(string codigo, string nome, int mes, int dia, DateOnly inicio, string fundamentoLegal, DateOnly? fim = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
            ArgumentException.ThrowIfNullOrWhiteSpace(nome);
            ArgumentException.ThrowIfNullOrWhiteSpace(fundamentoLegal);

            _ = new DateOnly(2000, mes, dia);
            ValidarVigencia(inicio, fim);

            return new RegraFeriadoEstadual(codigo, nome, mes, dia, null, inicio, fim, fundamentoLegal);
        }

        public static RegraFeriadoEstadual RelativaPascoa(string codigo, string nome, int deslocamento, DateOnly inicio, string fundamentoLegal, DateOnly? fim = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
            ArgumentException.ThrowIfNullOrWhiteSpace(nome);
            ArgumentException.ThrowIfNullOrWhiteSpace(fundamentoLegal);

            if (deslocamento < -365 || deslocamento > 365)
            {
                throw new ArgumentOutOfRangeException(nameof(deslocamento));
            }

            ValidarVigencia(inicio, fim);

            return new RegraFeriadoEstadual(codigo, nome, null, null, deslocamento, inicio, fim, fundamentoLegal);
        }

        public static RegraFeriadoEstadual FixaTransferidaDomingo(string codigo, string nome, int mes, int dia, DateOnly inicio, string fundamentoLegal, DateOnly? fim = null)
        {
            return Fixa(codigo, nome, mes, dia, inicio, fundamentoLegal, fim) with { TransferirDomingoSeguinte = true };
        }

        public DateOnly CalcularData(int ano, DateOnly pascoa)
        {
            if (DeslocamentoPascoa.HasValue)
            {
                return pascoa.AddDays(DeslocamentoPascoa.Value);
            }

            var data = new DateOnly(ano, Mes!.Value, Dia!.Value);

            if (TransferirDomingoSeguinte && data.DayOfWeek >= DayOfWeek.Monday && data.DayOfWeek <= DayOfWeek.Friday)
            {
                return data.AddDays(7 - (int)data.DayOfWeek);
            }

            return data;
        }

        public bool EstaVigente(DateOnly data)
        {
            return data >= VigenciaInicio && (!VigenciaFim.HasValue || data <= VigenciaFim.Value);
        }

        private static void ValidarVigencia(DateOnly inicio, DateOnly? fim)
        {
            if (fim.HasValue && fim.Value < inicio)
            {
                throw new ArgumentException("A vigência final não pode ser anterior à inicial.");
            }
        }
    }
}
