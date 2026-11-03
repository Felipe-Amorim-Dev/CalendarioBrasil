using CalendarioBrasil.Infrastructure.Calendarios.Models;
using CalendarioBrasil.Infrastructure.Calendarios.Regras;
using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarioBrasil.Infrastructure.Calendarios.Validacoes
{
    internal static class ValidadorAuditoriaEstadual
    {
        public static void Validar(AuditoriaEstadualJson? auditoria, IReadOnlyList<RegraFeriadoEstadual> regras, string uf)
        {
            if (auditoria is null)
            {
                return;
            }

            if (auditoria.Situacao is not ("Pendente" or "EmRevisao" or "Aprovada"))
            {
                throw new InvalidDataException($"Situação de auditoria inválida para {uf}.");
            }

            if (auditoria.FontesPorCodigo is null || auditoria.FontesConsultadas is null)
            {
                throw new InvalidDataException($"As fontes documentais do catálogo {uf} não podem ser nulas.");
            }

            var codigos = regras.Select(regra => regra.Codigo).ToHashSet(StringComparer.Ordinal);

            foreach (var fonte in auditoria.FontesPorCodigo)
            {
                if (!codigos.Contains(fonte.Key))
                {
                    throw new InvalidDataException($"A fonte {fonte.Key} não corresponde a uma regra cadastrada em {uf}.");
                }

                if (!EnderecoValido(fonte.Value))
                {
                    throw new InvalidDataException($"Endereço documental inválido para {fonte.Key}.");
                }
            }

            foreach (var fonte in auditoria.FontesConsultadas)
            {
                if (string.IsNullOrWhiteSpace(fonte.Key) || !EnderecoValido(fonte.Value))
                {
                    throw new InvalidDataException($"Fonte documental consultada inválida para {uf}.");
                }
            }

            if (!string.IsNullOrWhiteSpace(auditoria.ObservacaoCobertura) && auditoria.FontesConsultadas.Count == 0)
            {
                throw new InvalidDataException($"A observação de cobertura de {uf} exige uma fonte documental.");
            }

            if (auditoria.Situacao != "Aprovada")
            {
                return;
            }

            if (regras.Count == 0)
            {
                throw new InvalidDataException($"O catálogo {uf} não pode ser aprovado sem uma revisão específica de cobertura.");
            }

            if (!auditoria.DataRevisao.HasValue || string.IsNullOrWhiteSpace(auditoria.Responsavel))
            {
                throw new InvalidDataException($"A auditoria aprovada de {uf} exige data e responsável.");
            }

            if (auditoria.DataRevisao.Value > DateOnly.FromDateTime(DateTime.UtcNow))
            {
                throw new InvalidDataException($"A data de revisão de {uf} não pode estar no futuro.");
            }

            if (!auditoria.AnoInicialValidado.HasValue || !auditoria.AnoFinalValidado.HasValue || auditoria.AnoInicialValidado.Value < 2024 || auditoria.AnoFinalValidado.Value > 2100 || auditoria.AnoInicialValidado.Value > auditoria.AnoFinalValidado.Value)
            {
                throw new InvalidDataException($"O período validado de {uf} é inválido.");
            }

            if (regras.Any(regra => !regra.VigenciaInicio.HasValue))
            {
                throw new InvalidDataException($"O catálogo {uf} possui vigências legais não confirmadas.");
            }

            if (auditoria.FontesPorCodigo.Count != regras.Count)
            {
                throw new InvalidDataException($"A auditoria de {uf} exige uma fonte para cada regra.");
            }
        }

        private static bool EnderecoValido(string? endereco)
        {
            return !string.IsNullOrWhiteSpace(endereco) &&
                Uri.TryCreate(endereco, UriKind.Absolute, out var uri) &&
                uri.Scheme == Uri.UriSchemeHttps &&
                string.IsNullOrEmpty(uri.UserInfo);
        }
    }
}