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

            if (auditoria.FontesPorCodigo is null)
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

                if (string.IsNullOrWhiteSpace(fonte.Value) || !Uri.TryCreate(fonte.Value, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps || !string.IsNullOrEmpty(uri.UserInfo))
                {
                    throw new InvalidDataException($"Endereço documental inválido para {fonte.Key}.");
                }
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
    }
}
