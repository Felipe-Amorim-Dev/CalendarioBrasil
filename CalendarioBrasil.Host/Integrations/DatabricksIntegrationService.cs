using CalendarioBrasil.Application.DTOs;
using CalendarioBrasil.Application.Interfaces;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace CalendarioBrasil.Host.Integrations
{
    public sealed class DatabricksIntegrationService : IDatabricksIntegrationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public DatabricksIntegrationService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public bool Configurado => ObterHost() is not null &&
            !string.IsNullOrWhiteSpace(_configuration["Databricks:ClientId"]) &&
            !string.IsNullOrWhiteSpace(_configuration["Databricks:ClientSecret"]);

        public async Task<DatabricksTesteDto> TestarAsync(CancellationToken cancellationToken = default)
        {
            var host = ObterHost();
            var clientId = _configuration["Databricks:ClientId"];
            var clientSecret = _configuration["Databricks:ClientSecret"];
            var warehouseId = _configuration["Databricks:WarehouseId"];

            if (host is null || string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
            {
                return new DatabricksTesteDto(false, "A integração Databricks ainda não está configurada.");
            }

            try
            {
                using var autenticacao = new HttpRequestMessage(HttpMethod.Post, new Uri(host, "oidc/v1/token"));
                var credenciais = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
                autenticacao.Headers.Authorization = new AuthenticationHeaderValue("Basic", credenciais);
                autenticacao.Content = new FormUrlEncodedContent([
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("scope", "all-apis")
                ]);

                using var respostaToken = await _httpClient.SendAsync(autenticacao, cancellationToken);

                if (!respostaToken.IsSuccessStatusCode)
                {
                    return new DatabricksTesteDto(false, "Não foi possível autenticar a identidade de serviço no Databricks.");
                }

                using var documento = JsonDocument.Parse(await respostaToken.Content.ReadAsStringAsync(cancellationToken));

                if (!documento.RootElement.TryGetProperty("access_token", out var propriedadeToken))
                {
                    return new DatabricksTesteDto(false, "O Databricks não retornou um token de acesso válido.");
                }

                var token = propriedadeToken.GetString();

                if (string.IsNullOrWhiteSpace(token))
                {
                    return new DatabricksTesteDto(false, "O Databricks não retornou um token de acesso válido.");
                }

                if (string.IsNullOrWhiteSpace(warehouseId))
                {
                    return new DatabricksTesteDto(true, "Autenticação OAuth validada. Nenhum SQL Warehouse foi configurado para verificação.");
                }

                var caminho = $"api/2.0/sql/warehouses/{Uri.EscapeDataString(warehouseId.Trim())}";

                using var consulta = new HttpRequestMessage(HttpMethod.Get, new Uri(host, caminho));
                consulta.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                using var respostaWarehouse = await _httpClient.SendAsync(consulta, cancellationToken);

                if (respostaWarehouse.IsSuccessStatusCode)
                {
                    return new DatabricksTesteDto(true, "Conexão validada. A identidade de serviço possui acesso ao SQL Warehouse configurado.");
                }

                if (respostaWarehouse.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                {
                    return new DatabricksTesteDto(false, "Autenticação realizada, mas a identidade de serviço não possui permissão para consultar o SQL Warehouse.");
                }

                return new DatabricksTesteDto(false, "Autenticação realizada, mas não foi possível validar o SQL Warehouse informado.");
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                return new DatabricksTesteDto(false, "Tempo limite excedido ao tentar conectar ao Databricks.");
            }
            catch (HttpRequestException)
            {
                return new DatabricksTesteDto(false, "Não foi possível estabelecer comunicação HTTPS com o Databricks.");
            }
            catch (JsonException)
            {
                return new DatabricksTesteDto(false, "O Databricks retornou uma resposta de autenticação inválida.");
            }
        }

        private Uri? ObterHost()
        {
            var endereco = _configuration["Databricks:Host"];

            if (!Uri.TryCreate(endereco, UriKind.Absolute, out var uri))
            {
                return null;
            }

            if (uri.Scheme != Uri.UriSchemeHttps || !string.IsNullOrEmpty(uri.UserInfo) ||
                uri.AbsolutePath != "/" || !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment))
            {
                return null;
            }

            return uri;
        }
    }
}
