using CalendarioBrasil.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Domain.Entities
{
    public sealed class EventoCalendario
    {
        public string Codigo { get; }
        public string Nome { get; }
        public DateOnly Data { get; }
        public TipoEventoCalendario Tipo { get; }
        public EsferaAplicacaoEvento Esfera { get; }
        public string? Uf { get; }
        public string? CodigoMunicipioIbge { get; }
        public string? FundamentoLegal { get; }
        public DateOnly? VigenciaInicio { get; }
        public DateOnly? VigenciaFim { get; }
        public string? Observacao { get; }

        public EventoCalendario(string codigo, string nome, DateOnly data, TipoEventoCalendario tipo, EsferaAplicacaoEvento esfera, string? uf = null, string? codigoMunicipioIbge = null, string? fundamentoLegal = null, DateOnly? vigenciaInicio = null, DateOnly? vigenciaFim = null, string? observacao = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
            ArgumentException.ThrowIfNullOrWhiteSpace(nome);

            if (!Enum.IsDefined(tipo))
            {
                throw new ArgumentOutOfRangeException(nameof(tipo));
            }

            if (!Enum.IsDefined(esfera))
            {
                throw new ArgumentOutOfRangeException(nameof(esfera));
            }

            if (tipo != TipoEventoCalendario.DataComemorativa && string.IsNullOrWhiteSpace(fundamentoLegal))
            {
                throw new ArgumentException("Eventos oficiais exigem fundamento legal.", nameof(fundamentoLegal));
            }

            if (tipo == TipoEventoCalendario.PontoFacultativo && (!vigenciaInicio.HasValue || !vigenciaFim.HasValue))
            {
                throw new ArgumentException("Pontos facultativos exigem período de vigência explícito.");
            }

            if (vigenciaInicio.HasValue && vigenciaFim.HasValue && vigenciaInicio.Value > vigenciaFim.Value)
            {
                throw new ArgumentException("A data inicial da vigência não pode superar a data final.");
            }

            if ((vigenciaInicio.HasValue && data < vigenciaInicio.Value) || (vigenciaFim.HasValue && data > vigenciaFim.Value))
            {
                throw new ArgumentOutOfRangeException(nameof(data), "A ocorrência está fora do período de vigência.");
            }

            bool exigeUf = esfera is EsferaAplicacaoEvento.Estadual or EsferaAplicacaoEvento.Municipal;

            if (exigeUf && (string.IsNullOrWhiteSpace(uf) || uf.Trim().Length != 2 || !uf.Trim().All(char.IsLetter)))
            {
                throw new ArgumentException("Eventos estaduais e municipais exigem uma UF válida em formato.", nameof(uf));
            }

            if (!exigeUf && uf is not null)
            {
                throw new ArgumentException("A esfera informada não permite UF.", nameof(uf));
            }

            if (esfera == EsferaAplicacaoEvento.Municipal)
            {
                if (string.IsNullOrWhiteSpace(codigoMunicipioIbge) || codigoMunicipioIbge.Length != 7 || !codigoMunicipioIbge.All(char.IsAsciiDigit))
                {
                    throw new ArgumentException("Eventos municipais exigem código IBGE de sete dígitos.", nameof(codigoMunicipioIbge));
                }
            }
            else if (codigoMunicipioIbge is not null)
            {
                throw new ArgumentException("Código municipal permitido somente para eventos municipais.", nameof(codigoMunicipioIbge));
            }

            Codigo = codigo.Trim();
            Nome = nome.Trim();
            Data = data;
            Tipo = tipo;
            Esfera = esfera;
            Uf = uf?.Trim().ToUpperInvariant();
            CodigoMunicipioIbge = codigoMunicipioIbge;
            FundamentoLegal = fundamentoLegal;
            VigenciaInicio = vigenciaInicio;
            VigenciaFim = vigenciaFim;
            Observacao = observacao;
        }
    }
}
