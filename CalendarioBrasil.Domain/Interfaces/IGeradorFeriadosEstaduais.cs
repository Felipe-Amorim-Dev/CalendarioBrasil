using CalendarioBrasil.Domain.Entities;
using CalendarioBrasil.Domain.Enums;
using CalendarioBrasil.Domain.ValueObjects;

namespace CalendarioBrasil.Domain.Interfaces
{
    public interface IGeradorFeriadosEstaduais
    {
        IReadOnlyList<string> ObterUfsDisponiveis();
        IReadOnlyList<Feriado> Gerar(AnoCalendario ano, string uf, EscopoCalendarioEstadual escopo = EscopoCalendarioEstadual.Geral);
        DiagnosticoCatalogoEstadual ObterDiagnostico(string uf, int? ano = null);
    }
}