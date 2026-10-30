using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Infrastructure.Calendarios.Regras
{
    internal sealed record RegraFeriadoEstadual(string Codigo, string Nome, int? Mes, int? Dia, int? DeslocamentoPascoa, DateOnly? VigenciaInicio, DateOnly? VigenciaFim, string FundamentoLegal, DateOnly? InicioEvidencia)
    {
        public bool TransferirDomingoSeguinte { get; init; }
        public bool TransferirSextaFeira { get; init; }

        public static RegraFeriadoEstadual Fixa(string codigo, string nome, int mes, int dia, DateOnly? inicio, string fundamentoLegal, DateOnly? fim = null, DateOnly? inicioEvidencia = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
            ArgumentException.ThrowIfNullOrWhiteSpace(nome);
            ArgumentException.ThrowIfNullOrWhiteSpace(fundamentoLegal);

            _ = new DateOnly(2000, mes, dia);
            ValidarPeriodo(inicio, fim, inicioEvidencia);

            return new RegraFeriadoEstadual(codigo, nome, mes, dia, null, inicio, fim, fundamentoLegal, inicioEvidencia);
        }

        public static RegraFeriadoEstadual RelativaPascoa(string codigo, string nome, int deslocamento, DateOnly? inicio, string fundamentoLegal, DateOnly? fim = null, DateOnly? inicioEvidencia = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
            ArgumentException.ThrowIfNullOrWhiteSpace(nome);
            ArgumentException.ThrowIfNullOrWhiteSpace(fundamentoLegal);

            if (deslocamento < -365 || deslocamento > 365)
            {
                throw new ArgumentOutOfRangeException(nameof(deslocamento));
            }

            ValidarPeriodo(inicio, fim, inicioEvidencia);

            return new RegraFeriadoEstadual(codigo, nome, null, null, deslocamento, inicio, fim, fundamentoLegal, inicioEvidencia);
        }

        public static RegraFeriadoEstadual FixaTransferidaDomingo(string codigo, string nome, int mes, int dia, DateOnly? inicio, string fundamentoLegal, DateOnly? fim = null, DateOnly? inicioEvidencia = null)
        {
            return Fixa(codigo, nome, mes, dia, inicio, fundamentoLegal, fim, inicioEvidencia) with { TransferirDomingoSeguinte = true };
        }

        public static RegraFeriadoEstadual FixaTransferidaSexta(string codigo, string nome, int mes, int dia, DateOnly? inicio, string fundamentoLegal, DateOnly? fim = null, DateOnly? inicioEvidencia = null)
        {
            return Fixa(codigo, nome, mes, dia, inicio, fundamentoLegal, fim, inicioEvidencia) with { TransferirSextaFeira = true };
        }

        public DateOnly CalcularDataOriginal(int ano, DateOnly pascoa)
        {
            if (DeslocamentoPascoa.HasValue)
            {
                return pascoa.AddDays(DeslocamentoPascoa.Value);
            }

            return new DateOnly(ano, Mes!.Value, Dia!.Value);
        }

        public DateOnly CalcularData(int ano, DateOnly pascoa)
        {
            var data = CalcularDataOriginal(ano, pascoa);

            if (TransferirDomingoSeguinte && data.DayOfWeek >= DayOfWeek.Monday && data.DayOfWeek <= DayOfWeek.Friday)
            {
                return data.AddDays(7 - (int)data.DayOfWeek);
            }

            if (TransferirSextaFeira && data.DayOfWeek >= DayOfWeek.Tuesday && data.DayOfWeek <= DayOfWeek.Thursday)
            {
                return data.AddDays((int)DayOfWeek.Friday - (int)data.DayOfWeek);
            }

            return data;
        }

        public bool EstaVigente(DateOnly data)
        {
            var inicioAplicavel = VigenciaInicio ?? InicioEvidencia;

            return (!inicioAplicavel.HasValue || data >= inicioAplicavel.Value) && (!VigenciaFim.HasValue || data <= VigenciaFim.Value);
        }

        private static void ValidarPeriodo(DateOnly? inicio, DateOnly? fim, DateOnly? inicioEvidencia)
        {
            if (!inicio.HasValue && !inicioEvidencia.HasValue)
            {
                throw new ArgumentException("Informe a vigência legal ou o início da evidência documental.");
            }

            if (inicio.HasValue && fim.HasValue && inicio.Value > fim.Value)
            {
                throw new ArgumentException("A vigência final não pode ser anterior à inicial.");
            }

            if (inicioEvidencia.HasValue && fim.HasValue && inicioEvidencia.Value > fim.Value)
            {
                throw new ArgumentException("O início da evidência não pode ser posterior ao fim da vigência.");
            }
        }
    }
}
