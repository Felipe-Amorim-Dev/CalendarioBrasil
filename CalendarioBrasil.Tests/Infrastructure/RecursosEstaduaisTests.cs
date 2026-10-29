using CalendarioBrasil.Infrastructure.Calendarios;

namespace CalendarioBrasil.Tests.Infrastructure
{
    public sealed class RecursosEstaduaisTests
    {
        [Fact]
        public void Infrastructure_DeveConterCatalogosEstaduaisIncorporados()
        {
            var assembly = typeof(GeradorFeriadosEstaduais).Assembly;
            var recursos = assembly.GetManifestResourceNames();

            string[] esperados =
            [
                "CalendarioBrasil.Infrastructure.Catalogos.Estaduais.RJ.json",
                "CalendarioBrasil.Infrastructure.Catalogos.Estaduais.RS.json",
                "CalendarioBrasil.Infrastructure.Catalogos.Estaduais.SP.json"
            ];

            foreach (var esperado in esperados)
            {
                Assert.True(recursos.Contains(esperado), $"Recurso não encontrado: {esperado}. Recursos disponíveis: {string.Join(", ", recursos)}");
            }
        }
    }
}