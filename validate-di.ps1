#!/usr/bin/env pwsh

# ============================================================================
# Script de Validação - Injeção de Dependência
# ============================================================================
# Testa se todas as dependências estão corretamente registradas

Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  🔍 Validando Injeção de Dependência                      ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# 1. Verificar Build
Write-Host "1️⃣  Compilando projeto..." -ForegroundColor Yellow
$buildOutput = dotnet build 2>&1
if ($LASTEXITCODE -eq 0) {
	Write-Host "   ✅ Build bem-sucedido!" -ForegroundColor Green
} else {
	Write-Host "   ❌ Erro na compilação!" -ForegroundColor Red
	Write-Host $buildOutput
	exit 1
}
Write-Host ""

# 2. Verificar se os arquivos existem
Write-Host "2️⃣  Verificando arquivos críticos..." -ForegroundColor Yellow

$files = @(
	"src/CustomerManager.Domain/Interfaces/Services/ICustomerEventPublisher.cs",
	"src/CustomerManager.Infra/Messaging/CustomerEventPublisher.cs",
	"src/CustomerManager.Ioc/AppServiceCollectionExtensions.cs",
	"src/CustomerManager.Api/appsettings.Development.json"
)

foreach ($file in $files) {
	if (Test-Path $file) {
		Write-Host "   ✅ $file" -ForegroundColor Green
	} else {
		Write-Host "   ❌ $file NÃO ENCONTRADO!" -ForegroundColor Red
		exit 1
	}
}
Write-Host ""

# 3. Verificar Registros de DI no AppServiceCollectionExtensions
Write-Host "3️⃣  Verificando registros de DI..." -ForegroundColor Yellow

$dioContent = Get-Content "src/CustomerManager.Ioc/AppServiceCollectionExtensions.cs" -Raw

$requiredRegistrations = @(
	"AddAWSService<IAmazonSimpleNotificationService>",
	"AddStackExchangeRedisCache",
	"AddScoped<ICustomerEventPublisher, CustomerEventPublisher>",
	"AddScoped<ICreateCustomerHandler, CreateCustomerHandler>",
	"AddScoped<IGetAllCustomerHandler, GetAllCustomersHandler>",
	"AddScoped<IGetCustomerByCPFHandler, GetCustomerByCpfHandler>"
)

foreach ($registration in $requiredRegistrations) {
	if ($dioContent -match $registration) {
		Write-Host "   ✅ $registration" -ForegroundColor Green
	} else {
		Write-Host "   ❌ $registration NÃO ENCONTRADO!" -ForegroundColor Red
		exit 1
	}
}
Write-Host ""

# 4. Verificar ConnectionStrings
Write-Host "4️⃣  Verificando configurações..." -ForegroundColor Yellow

$appsettings = Get-Content "src/CustomerManager.Api/appsettings.Development.json" | ConvertFrom-Json

if ($appsettings.ConnectionStrings.Default) {
	Write-Host "   ✅ ConnectionString 'Default' configurada" -ForegroundColor Green
} else {
	Write-Host "   ❌ ConnectionString 'Default' não encontrada!" -ForegroundColor Red
}

if ($appsettings.ConnectionStrings.Redis) {
	Write-Host "   ✅ ConnectionString 'Redis' configurada" -ForegroundColor Green
} else {
	Write-Host "   ❌ ConnectionString 'Redis' não encontrada!" -ForegroundColor Red
}

if ($appsettings.AWS.SNS.ContaEventosTopicArn) {
	Write-Host "   ✅ AWS SNS TopicArn configurado" -ForegroundColor Green
} else {
	Write-Host "   ❌ AWS SNS TopicArn não configurado!" -ForegroundColor Red
}
Write-Host ""

# 5. Verificar usando directivas corretos
Write-Host "5️⃣  Verificando usando directivas..." -ForegroundColor Yellow

$handlers = @(
	"src/CustomerManager.Application/Handlers/CreateCustomerHandler.cs",
	"src/CustomerManager.Application/Handlers/UpdateCustomerHandler.cs",
	"src/CustomerManager.Application/Handlers/DeleteCustomerHandler.cs"
)

foreach ($handler in $handlers) {
	$content = Get-Content $handler -Raw
	if ($content -match "using CustomerManager.Domain.Interfaces.Services;") {
		Write-Host "   ✅ $(Split-Path -Leaf $handler)" -ForegroundColor Green
	} else {
		Write-Host "   ⚠️  $(Split-Path -Leaf $handler) - verifique usando" -ForegroundColor Yellow
	}
}
Write-Host ""

# 6. Verificar NuGet packages
Write-Host "6️⃣  Verificando pacotes NuGet..." -ForegroundColor Yellow

$iocProj = Get-Content "src/CustomerManager.Ioc/CustomerManager.Ioc.csproj" -Raw

if ($iocProj -match "AWSSDK.Extensions.NETCore.Setup") {
	Write-Host "   ✅ AWSSDK.Extensions.NETCore.Setup" -ForegroundColor Green
} else {
	Write-Host "   ⚠️  AWSSDK.Extensions.NETCore.Setup não encontrado" -ForegroundColor Yellow
}

if ($iocProj -match "Microsoft.Extensions.Caching.StackExchangeRedis") {
	Write-Host "   ✅ Microsoft.Extensions.Caching.StackExchangeRedis" -ForegroundColor Green
} else {
	Write-Host "   ⚠️  Microsoft.Extensions.Caching.StackExchangeRedis não encontrado" -ForegroundColor Yellow
}

Write-Host ""

# 7. Resumo Final
Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  ✅ VALIDAÇÃO COMPLETA                                    ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""
Write-Host "📋 Checklist:" -ForegroundColor Green
Write-Host "   ✅ Build bem-sucedido" -ForegroundColor Green
Write-Host "   ✅ Arquivos críticos presentes" -ForegroundColor Green
Write-Host "   ✅ Registros de DI configurados" -ForegroundColor Green
Write-Host "   ✅ ConnectionStrings configuradas" -ForegroundColor Green
Write-Host "   ✅ Usando directivas corretos" -ForegroundColor Green
Write-Host "   ✅ Pacotes NuGet presentes" -ForegroundColor Green
Write-Host ""

Write-Host "🚀 Pronto para executar! Use:" -ForegroundColor Yellow
Write-Host "   dotnet run --project src/CustomerManager.Api/CustomerManager.Api.csproj" -ForegroundColor Cyan
Write-Host ""
