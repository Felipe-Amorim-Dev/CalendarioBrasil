using CalendarioBrasil.Domain.Entities;
using CalendarioBrasil.Domain.ValueObjects;

namespace CalendarioBrasil.Domain.Interfaces
{
    public interface IGeradorFeriadosEstaduais
    {
        IReadOnlyList<string> ObterUfsDisponiveis();
        IReadOnlyList<Feriado> Gerar(AnoCalendario ano, string uf);
        DiagnosticoCatalogoEstadual ObterDiagnostico(string uf, int? ano = null);
    }
}