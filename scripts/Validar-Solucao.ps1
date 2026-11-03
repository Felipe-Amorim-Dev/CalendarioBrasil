param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuracao = "Release",
    [switch]$SemCobertura
)

$ErrorActionPreference = "Stop"

$raiz = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$solucao = Join-Path $raiz "CalendarioBrasil.slnx"
$projetoTestes = Join-Path $raiz "CalendarioBrasil.Tests\CalendarioBrasil.Tests.csproj"
$resultados = Join-Path $raiz "TestResults"
$execucao = Join-Path $resultados (Get-Date -Format "yyyyMMdd-HHmmss")

function Executar-Dotnet {
    param([string]$Etapa, [string[]]$Argumentos)

    Write-Host "`n$Etapa" -ForegroundColor Cyan
    & dotnet @Argumentos

    if ($LASTEXITCODE -ne 0) {
        throw "Falha na etapa '$Etapa'. Código de saída: $LASTEXITCODE."
    }
}

Write-Host "CalendarioBrasil - Validacao da Solucao" -ForegroundColor Cyan

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw "O SDK .NET nao foi encontrado no ambiente."
}

$sdks = @(& dotnet --list-sdks)

if ($LASTEXITCODE -ne 0 -or -not ($sdks | Where-Object { $_ -match '^10\.' })) {
    throw "O SDK .NET 10 nao esta instalado."
}

if (-not (Test-Path -LiteralPath $solucao -PathType Leaf)) {
    throw "Arquivo da solucao nao encontrado: $solucao"
}

if (-not (Test-Path -LiteralPath $projetoTestes -PathType Leaf)) {
    throw "Projeto de testes nao encontrado: $projetoTestes"
}

New-Item -ItemType Directory -Path $execucao -Force | Out-Null

Write-Host "SDK ativo: $(& dotnet --version)"
Write-Host "Configuracao: $Configuracao"

Executar-Dotnet "Restaurando dependencias" @("restore", $solucao)

Executar-Dotnet "Compilando solucao" @(
    "build", $solucao, "--configuration", $Configuracao, "--no-restore"
)

$argumentosTeste = @(
    "test", $projetoTestes,
    "--configuration", $Configuracao,
    "--no-build", "--no-restore",
    "--logger", "trx;LogFileName=CalendarioBrasil.Tests.trx",
    "--results-directory", $execucao
)

if (-not $SemCobertura) {
    $argumentosTeste += @("--collect", "XPlat Code Coverage")
}

Executar-Dotnet "Executando testes automatizados" $argumentosTeste

$relatorio = Join-Path $execucao "CalendarioBrasil.Tests.trx"

if (-not (Test-Path -LiteralPath $relatorio)) {
    throw "O relatorio TRX nao foi gerado."
}

if (-not $SemCobertura) {
    $coberturas = @(Get-ChildItem -Path $execucao -Filter "coverage.cobertura.xml" -File -Recurse)

    if ($coberturas.Count -eq 0) {
        throw "O relatorio de cobertura nao foi gerado."
    }
}

Write-Host "`nValidacao concluida com sucesso." -ForegroundColor Green
Write-Host "Resultados: $execucao"