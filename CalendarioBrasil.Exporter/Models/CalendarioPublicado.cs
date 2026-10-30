using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Exporter.Models
{
    public sealed record CalendarioPublicado(int Ano, string Uf, string Arquivo, string Cobertura, int Total, string Sha256, string Escopo = "Geral", string? ArquivoDatabricks = null, string? Sha256Databricks = null);
}
