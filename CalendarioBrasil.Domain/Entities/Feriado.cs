using CalendarioBrasil.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Domain.Entities
{
    public sealed class Feriado
    {
        public string Codigo { get; }
        public string Nome { get; }
        public DateOnly Data { get; }
        public DateOnly DataOriginal { get; }
        public bool Transferido => DataOriginal != Data;
        public AbrangenciaFeriado Abrangencia { get; }
        public string? Uf { get; }
        public string FundamentoLegal { get; }
        public SituacaoDataFeriado SituacaoData { get; }

        public Feriado(string codigo, string nome, DateOnly data, AbrangenciaFeriado abrangencia, string? uf, string fundamentoLegal, DateOnly? dataOriginal = null, SituacaoDataFeriado situacaoData = SituacaoDataFeriado.Calculada)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
            ArgumentException.ThrowIfNullOrWhiteSpace(nome);
            ArgumentException.ThrowIfNullOrWhiteSpace(fundamentoLegal);

            if (!Enum.IsDefined(abrangencia))
            {
                throw new ArgumentOutOfRangeException(nameof(abrangencia));
            }

            if (abrangencia == AbrangenciaFeriado.Nacional && uf is not null)
            {
                throw new ArgumentException("Feriados nacionais não devem possuir UF.", nameof(uf));
            }

            if (abrangencia != AbrangenciaFeriado.Nacional && string.IsNullOrWhiteSpace(uf))
            {
                throw new ArgumentException("Feriados regionais devem possuir UF.", nameof(uf));
            }

            if (!Enum.IsDefined(situacaoData))
            {
                throw new ArgumentOutOfRangeException(nameof(situacaoData));
            }

            Codigo = codigo;
            Nome = nome;
            Data = data;
            DataOriginal = dataOriginal ?? data;
            Abrangencia = abrangencia;
            Uf = uf;
            FundamentoLegal = fundamentoLegal;
            SituacaoData = situacaoData;
        }
    }
}
