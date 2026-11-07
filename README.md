<img src="doc/Calendário Brasil_ Banner.png" alt="Capa" width="1920px" align="center">

# CalendarioBrasil

**API REST para cálculo e consulta de feriados nacionais e estaduais brasileiros, desenvolvida em C# e .NET 10.**

O CalendarioBrasil centraliza regras de calendário, cálculos de datas móveis e catálogos estaduais, disponibilizando informações por meio de endpoints HTTP e ferramentas de exportação.

O projeto oferece suporte a consultas históricas e projeções futuras, seleção de múltiplas Unidades Federativas (UFs), identificação de abrangência e fundamentos legais, além de mecanismos para acompanhar a cobertura dos catálogos.

## Funcionalidades

### Feriados nacionais

- Geração de feriados nacionais de datas fixas.
- Cálculo da Paixão de Cristo (Sexta-feira Santa).
- Identificação de nome, data e fundamento legal.
- Ordenação cronológica.
- Consulta sem necessidade de informar uma UF.

### Feriados estaduais

- Catálogos organizados pelas 27 Unidades Federativas.
- Consulta por uma ou múltiplas UFs.
- Aplicação de regras estaduais cadastradas.
- Tratamento de vigências legais e exceções anuais.
- Suporte a transferências de datas conforme as regras disponíveis.
- Identificação de feriados estaduais coincidentes com nacionais.
- Avisos sobre a cobertura e as limitações dos catálogos.

A existência de um catálogo não significa que todas as regras estaduais estejam juridicamente homologadas.

### Consultas históricas e futuras

A API permite consultar anos entre **1583 e 9999**, utilizando cálculos compatíveis com o calendário gregoriano.

Entre os recursos disponíveis estão:

- Cálculo dinâmico de datas móveis.
- Consultas anteriores ao ano vigente.
- Projeções para anos futuros.
- Aplicação de vigências legais cadastradas.
- Identificação de limitações históricas e projeções.

**Importante:** os resultados históricos podem representar projeções retroativas do catálogo atual, não necessariamente uma reconstrução integral da legislação do período.

Da mesma forma, feriados futuros estão sujeitos a modificações legislativas.

### Escopos estaduais

Algumas regras de calendário possuem abrangências específicas. A API permite selecionar os escopos:

| Escopo | Descrição |
|--------|-----------|
| `Geral` | Calendário de abrangência geral |
| `ExecutivoEstadual` | Regras específicas do Executivo estadual |
| `JudiciarioEstadual` | Regras específicas do Judiciário estadual |

A disponibilidade de datas específicas depende dos registros existentes para cada UF e ano.

### Datas móveis

A API calcula datas relacionadas à Páscoa, incluindo:

- Carnaval.
- Quarta-feira de Cinzas.
- Sexta-feira Santa.
- Páscoa.
- Corpus Christi.

O cálculo de uma data móvel não significa, por si só, que ela seja um feriado nacional civil.

### Eventos complementares

A solução também contempla consultas de eventos de calendário, com opção de incluir pontos facultativos federais cadastrados.

### Cobertura estadual

A API disponibiliza informações sobre:

- Quantidade de regras cadastradas.
- Situação da cobertura estadual.
- Homologação das regras.
- Vigências não confirmadas.
- Anos revisados.
- Fontes legais cadastradas.
- Limitações e observações.

### Integração Databricks

O projeto possui uma integração inicial com Databricks por meio do backend .NET.

São disponibilizados recursos para:

- Consultar o estado da configuração.
- Validar a autenticação OAuth Machine-to-Machine (M2M).
- Verificar o acesso a um SQL Warehouse configurado.

O Exporter também pode gerar arquivos de registros preparados para uso em fluxos de dados com Databricks.

A integração atual não realiza automaticamente a gravação de feriados em tabelas Delta.

## Tecnologias utilizadas

| Tecnologia | Aplicação |
|------------|-----------|
| C# | Linguagem principal |
| .NET 10 | Plataforma de desenvolvimento |
| ASP.NET Core | Exposição dos endpoints REST |
| JSON | Catálogos estaduais e formatos de exportação |
| DateOnly | Representação de datas |
| xUnit | Testes automatizados |
| Coverlet | Coleta de cobertura de testes |
| PowerShell | Automação da validação da solução |
| Docker | Containerização da API |
| Docker Compose | Execução e configuração do container |
| OAuth M2M | Autenticação da integração Databricks |

## Arquitetura

A solução utiliza separação por camadas, isolando regras de negócio, serviços de aplicação, catálogos, contratos HTTP e hospedagem.

### Estrutura da solução

```text
CalendarioBrasil/
|
+-- CalendarioBrasil.Api/
|   +-- Controllers/
|
+-- CalendarioBrasil.Application/
|   +-- Constants/
|   +-- DTOs/
|   +-- Interfaces/
|   +-- Services/
|
+-- CalendarioBrasil.Domain/
|   +-- Entities/
|   +-- Enums/
|   +-- Interfaces/
|   +-- Services/
|   +-- ValueObjects/
|
+-- CalendarioBrasil.Infrastructure/
|   +-- Calendarios/
|   +-- Catalogos/
|
+-- CalendarioBrasil.Host/
|   +-- Integrations/
|   +-- Program.cs
|
+-- CalendarioBrasil.Exporter/
|   +-- Services/
|   +-- Site/
|
+-- CalendarioBrasil.Tests/
|
+-- scripts/
|   +-- Validar-Solucao.ps1
|
+-- Dockerfile
+-- docker-compose.yml
+-- CalendarioBrasil.slnx
+-- README.md
+-- LICENSE
```

### Responsabilidades

**Domain**

Contém as entidades, objetos de valor e serviços responsáveis pelas regras de calendário e pelo cálculo de feriados.

**Application**

Coordena os casos de uso, disponibiliza DTOs e define os contratos utilizados pelos serviços e pelos endpoints.

**Infrastructure**

Implementa a leitura e o processamento dos catálogos estaduais, incluindo regras de vigência, transferências e cobertura.

**API**

Disponibiliza os controllers HTTP, recebe os parâmetros das consultas e aplica as validações de entrada.

**Host**

Inicializa o ASP.NET Core, registra as dependências, configura os middlewares e hospeda a aplicação. Também contém a implementação de integração com Databricks.

**Exporter**

Permite gerar calendários em arquivos JSON, individualmente ou em lote, além de disponibilizar recursos para visualização dos arquivos exportados.

**Tests**

Contém os testes automatizados responsáveis pela validação das regras e dos componentes da solução.

### Fluxo de uma consulta

```text
Cliente HTTP
    |
    v
CalendarioBrasil.Api
    |
    v
CalendarioBrasil.Application
    |
    +-----------------------+
    |                       |
    v                       v
CalendarioBrasil.Domain    Infrastructure
    |                       |
    |                       +-- Catálogos estaduais
    |                       +-- Regras de vigência
    |                       +-- Exceções e transferências
    |
    +-- Feriados nacionais
    +-- Datas móveis
    +-- Regras de calendário
    |
    v
Resposta JSON
```

## Endpoints REST

A API utiliza o prefixo `/api/v1` para seus recursos principais.

| Método | Endpoint | Finalidade |
|--------|----------|------------|
| GET | `/api/v1/feriados` | Consultar feriados nacionais e estaduais |
| GET | `/api/v1/feriados/nacionais/fixos` | Consultar somente os nacionais fixos |
| GET | `/api/v1/datas-moveis` | Calcular datas móveis |
| GET | `/api/v1/eventos` | Consultar eventos e pontos facultativos disponíveis |
| GET | `/api/v1/cobertura-estadual` | Consultar a cobertura de um catálogo estadual |
| GET | `/api/v1/databricks/status` | Consultar o estado da integração |
| POST | `/api/v1/databricks/testar` | Testar conexão Databricks em desenvolvimento |
| GET | `/health` | Verificar a disponibilidade da API |

### Consultar feriados nacionais

```http
GET /api/v1/feriados?ano=2026
```

Quando nenhuma UF é informada, a resposta inclui somente os feriados nacionais calculados para o ano.

### Consultar uma UF

```http
GET /api/v1/feriados?ano=2026&uf=RJ
```

A resposta reúne os feriados nacionais e os estaduais do Rio de Janeiro cadastrados para o período e escopo consultados.

### Consultar múltiplas UFs

```http
GET /api/v1/feriados?ano=2026&uf=RJ,SP,MG
```

Também são aceitos parâmetros repetidos:

```http
GET /api/v1/feriados?ano=2026&uf=RJ&uf=SP&uf=MG
```

Os feriados nacionais são gerados uma única vez e combinados com os registros das UFs solicitadas.

### Consultar outro ano

Consulta histórica:

```http
GET /api/v1/feriados?ano=1900
```

Projeção futura:

```http
GET /api/v1/feriados?ano=2130&uf=RJ,SP
```

### Consultar um escopo específico

```http
GET /api/v1/feriados?ano=2026&uf=TO&escopo=ExecutivoEstadual
```

### Consultar datas móveis

```http
GET /api/v1/datas-moveis?ano=2026
```

### Consultar eventos

```http
GET /api/v1/eventos?ano=2026&uf=RJ
```

Para incluir pontos facultativos federais cadastrados:

```http
GET /api/v1/eventos?ano=2026&uf=RJ&incluirPontosFacultativosFederais=true
```

O endpoint de eventos recebe uma UF obrigatória, diferentemente do endpoint principal de feriados, que aceita múltiplas UFs.

### Consultar cobertura estadual

```http
GET /api/v1/cobertura-estadual?uf=RJ&ano=2026
```

O ano é opcional nesse endpoint.

### Verificar disponibilidade

```http
GET /health
```

Exemplo de resposta:

```json
{
  "status": "healthy"
}
```

## Estrutura da resposta de feriados

O endpoint principal retorna um objeto JSON contendo:

| Campo | Descrição |
|-------|-----------|
| `ano` | Ano solicitado |
| `uf` | UF consultada quando houver uma única UF |
| `ufs` | UFs selecionadas |
| `escopo` | Escopo aplicado na consulta |
| `cobertura` | Identificação da cobertura dos resultados |
| `total` | Quantidade de registros de feriados |
| `feriados` | Coleção de feriados calculados |
| `limitacoes` | Avisos e limitações da consulta |
| `alertas` | Alertas identificados no calendário |

Cada feriado contém informações como código, nome, data, abrangência, UF, fundamento legal e eventuais informações de transferência.

## Requisitos

Para desenvolver e executar localmente:

- SDK .NET 10.
- Git.
- Terminal compatível com os comandos do .NET CLI.
- PowerShell 7 para executar o script completo de validação.

Para execução containerizada:

- Docker Engine ou Docker Desktop.
- Docker Compose.

O cálculo dos feriados utiliza catálogos incorporados à aplicação e não depende de um banco de dados externo.

## Instalação e execução

### 1. Clonar o repositório

```bash
git clone https://github.com/Felipe-Amorim-Dev/CalendarioBrasil.git

cd CalendarioBrasil
```

### 2. Restaurar dependências

```bash
dotnet restore CalendarioBrasil.slnx
```

### 3. Compilar a solução

```bash
dotnet build CalendarioBrasil.slnx --configuration Release
```

### 4. Executar a API

```bash
dotnet run --project CalendarioBrasil.Host --launch-profile http
```

O perfil HTTP de desenvolvimento utiliza:

```text
http://localhost:5082
```

Teste a aplicação:

```bash
curl "http://localhost:5082/health"
```

Consulte um calendário:

```bash
curl "http://localhost:5082/api/v1/feriados?ano=2026&uf=RJ,SP"
```

## Execução com Docker

O repositório contém um Dockerfile para publicação da aplicação .NET 10 e um arquivo Docker Compose para execução local.

### Construir e iniciar

```bash
docker compose up -d --build
```

### Consultar os containers

```bash
docker compose ps
```

### Visualizar logs

```bash
docker compose logs -f api
```

### Encerrar

```bash
docker compose down
```

A configuração atual disponibiliza a API em:

```text
http://localhost:5082
```

O mapeamento de portas utiliza o endereço `127.0.0.1`, restringindo o acesso à máquina local.

O container utiliza a porta interna `8080` e executa a aplicação no ambiente `Production`.

## Exportação de calendários

O projeto `CalendarioBrasil.Exporter` disponibiliza uma ferramenta de linha de comando para gerar calendários em JSON.

### Exportação individual

```bash
dotnet run --project CalendarioBrasil.Exporter -- \
  --ano 2026 \
  --uf RJ \
  --saida dist
```

### Exportação em lote

```bash
dotnet run --project CalendarioBrasil.Exporter -- \
  --ano-inicial 2026 \
  --ano-final 2030 \
  --ufs todas \
  --saida dist
```

O Exporter também permite definir escopos de calendário e gerar arquivos adicionais preparados para fluxos de dados com Databricks.

Exemplo:

```bash
dotnet run --project CalendarioBrasil.Exporter -- \
  --ano 2026 \
  --uf RJ \
  --saida dist \
  --databricks true
```

A opção `--databricks true` gera arquivos de registros para integração analítica. Ela não realiza, por si só, uma conexão nem grava dados diretamente em um workspace Databricks.

## Integração Databricks

A API contém uma integração de diagnóstico com o Databricks.

### Autenticação

A implementação utiliza OAuth Machine-to-Machine (M2M), com credenciais configuradas exclusivamente no backend.

As configurações incluem:

| Chave | Finalidade |
|-------|------------|
| `Databricks:Host` | Endereço HTTPS do workspace |
| `Databricks:ClientId` | Identificador da identidade de serviço |
| `Databricks:ClientSecret` | Segredo OAuth da identidade |
| `Databricks:WarehouseId` | Identificador opcional do SQL Warehouse |

### Configuração local

Inicialize o armazenamento de segredos do projeto Host:

```bash
dotnet user-secrets init --project CalendarioBrasil.Host
```

Configure as informações não sensíveis utilizando User Secrets ou outro provedor de configuração do .NET.

O `ClientSecret` deve ser armazenado em um mecanismo seguro, sem inclusão em arquivos versionados ou no código-fonte.

### Verificar a configuração

```http
GET /api/v1/databricks/status
```

### Testar a conexão

```http
POST /api/v1/databricks/testar
```

O endpoint de teste está disponível somente em `Development`. Em outros ambientes, a implementação retorna HTTP 404.

A validação de conectividade não representa autorização para operações de escrita. Funcionalidades administrativas exigem controles adicionais de autenticação e autorização antes de exposição pública.

## Testes automatizados

O projeto utiliza xUnit para testar as regras e serviços da solução.

### Executar os testes

```bash
dotnet test CalendarioBrasil.Tests/CalendarioBrasil.Tests.csproj --configuration Release
```

### Validar toda a solução

O repositório possui um script PowerShell que executa:

1. Restauração das dependências.
2. Compilação da solução.
3. Testes automatizados.
4. Geração de relatório TRX.
5. Coleta de cobertura de código.

Execute:

```powershell
pwsh -NoProfile -File ./scripts/Validar-Solucao.ps1
```

Para executar sem coleta de cobertura:

```powershell
pwsh -NoProfile -File ./scripts/Validar-Solucao.ps1 -SemCobertura
```

Os resultados são armazenados em um diretório `TestResults`, organizado por execução.

## Validação de parâmetros e disponibilidade

A API utiliza validação de parâmetros para rejeitar entradas inválidas.

Entre os comportamentos implementados estão:

- Validação do intervalo de anos suportado.
- Validação das siglas das UFs.
- Validação dos escopos disponíveis.
- Respostas HTTP 400 para parâmetros inválidos.
- Tratamento centralizado de exceções.
- Limitação de requisições por janela de tempo.

O Host configura um limite de 60 requisições por minuto para a política principal dos endpoints HTTP. Quando o limite é excedido, a API pode responder com HTTP 429.

## Cobertura legal e limitações

O CalendarioBrasil foi desenvolvido para calcular calendários a partir das normas e regras cadastradas, mas **não deve ser interpretado como uma base jurídica integralmente homologada**.

Os principais aspectos são:

- A legislação pode sofrer alterações ao longo do tempo.
- Feriados estaduais podem possuir regras de vigência e transferência.
- Algumas datas podem depender de legislação municipal.
- Calendários administrativos podem divergir de feriados civis gerais.
- O conjunto nacional aplicado em consultas antigas pode representar uma projeção retroativa.
- A ausência de uma regra cadastrada não comprova a inexistência de um feriado.

A API disponibiliza avisos de cobertura e informações sobre pendências para permitir que sistemas consumidores identifiquem essas limitações.

**Os resultados devem ser confirmados em fontes oficiais quando utilizados para finalidades jurídicas, trabalhistas ou administrativas.**

## Front-end — CalendarioBrasil-Web

A API é utilizada pelo projeto CalendarioBrasil-Web, desenvolvido em Angular 22.

O front-end oferece:

- Calendário anual com os 12 meses.
- Seleção de uma ou múltiplas UFs.
- Filtros por abrangência nacional ou estadual.
- Identificação visual de feriados.
- Detalhamento das datas e fundamentos legais.
- Avisos de cobertura e projeções.
- Painel de diagnóstico Databricks.

**Repositório:**

[CalendarioBrasil-Web](https://github.com/Felipe-Amorim-Dev/CalendarioBrasil-Web)

## Licença

Distribuído sob a licença **MIT**.

Consulte o arquivo [LICENSE](LICENSE) para mais informações.

---

**CalendarioBrasil** — Cálculo e consulta de feriados brasileiros com .NET 10, regras de calendário e suporte a integração com aplicações externas.