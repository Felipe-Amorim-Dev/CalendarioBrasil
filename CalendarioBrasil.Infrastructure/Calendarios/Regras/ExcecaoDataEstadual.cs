using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Infrastructure.Calendarios.Regras
{
    internal sealed record ExcecaoDataEstadual(string Uf, int Ano, string Codigo, DateOnly DataOriginal, DateOnly DataEfetiva, string Escopo, string FundamentoLegal, string FonteOficial);
}
