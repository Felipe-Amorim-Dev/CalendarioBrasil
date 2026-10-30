using CalendarioBrasil.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Exporter.Models
{
    public sealed record EventoDatabricks(int Ano, string Uf, string Escopo, string Cobertura, string Codigo, string Nome, DateOnly Data, DateOnly? DataOriginal, bool Transferido, string SituacaoData, string Tipo, string Esfera, string? UfEvento, string? CodigoMunicipioIbge, string? FundamentoLegal, DateOnly? VigenciaInicio, DateOnly? VigenciaFim, string? Observacao)
    {
        public static EventoDatabricks Criar(CalendarioEventosDto calendario, EventoCalendarioDto evento)
        {
            return new EventoDatabricks(calendario.Ano, calendario.Uf, calendario.Escopo, calendario.Cobertura, evento.Codigo, evento.Nome, evento.Data, evento.DataOriginal, evento.Transferido, evento.SituacaoData, evento.Tipo, evento.Esfera, evento.Uf, evento.CodigoMunicipioIbge, evento.FundamentoLegal, evento.VigenciaInicio, evento.VigenciaFim, evento.Observacao);
        }
    }
}
