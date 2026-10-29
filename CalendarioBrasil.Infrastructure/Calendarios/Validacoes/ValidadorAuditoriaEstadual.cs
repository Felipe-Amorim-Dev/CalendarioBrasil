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

            if (auditoria.Situacao != "Aprovada")
            {
                return;
            }

            if (regras.Count == 0)
            {
                throw new InvalidDataException($"O catálogo {uf} não pode ser aprovado automaticamente sem regras. É necessária uma revisão específica.");
            }

            if (!auditoria.DataRevisao.HasValue || string.IsNullOrWhiteSpace(auditoria.Responsavel))
            {
                throw new InvalidDataException($"A auditoria aprovada de {uf} exige data de revisão e responsável.");
            }

            if (auditoria.DataRevisao.Value > DateOnly.FromDateTime(DateTime.UtcNow))
            {
                throw new InvalidDataException($"A data de revisão de {uf} não pode estar no futuro.");
            }

            if (!auditoria.AnoInicialValidado.HasValue || !auditoria.AnoFinalValidado.HasValue || auditoria.AnoInicialValidado.Value < 2024 || auditoria.AnoFinalValidado.Value > 2100 || auditoria.AnoInicialValidado.Value > auditoria.AnoFinalValidado.Value)
            {
                throw new InvalidDataException($"A auditoria aprovada de {uf} exige um intervalo de anos válido.");
            }

            if (regras.Any(regra => !regra.VigenciaInicio.HasValue))
            {
                throw new InvalidDataException($"O catálogo {uf} possui regras com vigência legal não confirmada.");
            }

            if (auditoria.FontesPorCodigo is null || auditoria.FontesPorCodigo.Count != regras.Count)
            {
                throw new InvalidDataException($"A auditoria de {uf} deve possuir uma fonte para cada regra cadastrada.");
            }

            foreach (var regra in regras)
            {
                if (!auditoria.FontesPorCodigo.TryGetValue(regra.Codigo, out var endereco) || !Uri.TryCreate(endereco, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps || !string.IsNullOrEmpty(uri.UserInfo))
                {
                    throw new InvalidDataException($"Fonte documental inválida ou ausente para {regra.Codigo}.");
                }
            }
        }
    }
}
