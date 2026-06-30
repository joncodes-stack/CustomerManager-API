#!/usr/bin/env pwsh

# ============================================================================
# Script para Limpeza e Execução do AppHost
# ============================================================================
# Este script resolve problemas comuns com Aspire Hosting

param(
	[string]$Action = "help"
)

# Cores
$Green = [System.ConsoleColor]::Green
$Red = [System.ConsoleColor]::Red
$Yellow = [System.ConsoleColor]::Yellow
$Blue = [System.ConsoleColor]::Cyan

function Print-Header {
	param([string]$Message)
	Write-Host "╔════════════════════════════════════════╗" -ForegroundColor $Blue
	Write-Host "║ $Message" -ForegroundColor $Blue
	Write-Host "╚════════════════════════════════════════╝" -ForegroundColor $Blue
}

function Print-Success {
	param([string]$Message)
	Write-Host "✓ $Message" -ForegroundColor $Green
}

function Print-Error {
	param([string]$Message)
	Write-Host "✗ $Message" -ForegroundColor $Red
}

function Print-Warning {
	param([string]$Message)
	Write-Host "⚠ $Message" -ForegroundColor $Yellow
}

function Clean-AppHost {
	Print-Header "🧹 Limpando AppHost"

	Write-Host "`nLimpando arquivos de build..."

	$paths = @(
		"src/CustomerManager.AppHost/CustomerManager.AppHost.AppHost/bin",
		"src/CustomerManager.AppHost/CustomerManager.AppHost.AppHost/obj",
		"src/CustomerManager.AppHost/CustomerManager.AppHost.ServiceDefaults/bin",
		"src/CustomerManager.AppHost/CustomerManager.AppHost.ServiceDefaults/obj"
	)

	foreach ($path in $paths) {
		if (Test-Path $path) {
			Remove-Item -Path $path -Recurse -Force -ErrorAction SilentlyContinue
			Print-Success "Removido: $path"
		}
	}

	Write-Host "`nLimpando solução..."
	dotnet clean
	Print-Success "Limpeza completa concluída!"
}

function Restore-Packages {
	Print-Header "📦 Restaurando pacotes NuGet"

	Write-Host "`nLimpando cache NuGet..."
	dotnet nuget locals all --clear
	Print-Success "Cache NuGet limpo!"

	Write-Host "`nRestaurando pacotes..."
	dotnet restore

	if ($LASTEXITCODE -eq 0) {
		Print-Success "Pacotes restaurados com sucesso!"
	} else {
		Print-Error "Erro ao restaurar pacotes"
		exit 1
	}
}

function Build-Solution {
	Print-Header "🔨 Compilando solução"

	dotnet build

	if ($LASTEXITCODE -eq 0) {
		Print-Success "Build concluído com sucesso!"
	} else {
		Print-Error "Erro na compilação"
		exit 1
	}
}

function Run-AppHost {
	Print-Header "🚀 Iniciando AppHost"

	Print-Warning "AppHost iniciado na porta 8000"
	Print-Warning "Dashboard disponível em: http://localhost:8000"

	dotnet run --project "src/CustomerManager.AppHost/CustomerManager.AppHost.AppHost/CustomerManager.AppHost.csproj"
}

function Clean-Docker {
	Print-Header "🐳 Limpando containers Docker"

	Print-Warning "Removendo containers Aspire..."

	$containers = @(
		"app-postgres",
		"app-redis",
		"app-localstack",
		"postgres",
		"redis",
		"localstack"
	)

	foreach ($container in $containers) {
		$existing = docker ps -a --filter "name=$container" -q
		if ($existing) {
			docker rm -f $existing
			Print-Success "Removido container: $container"
		}
	}

	Print-Success "Limpeza de containers concluída!"
}

function Full-Setup {
	Print-Header "⚡ Setup Completo"

	Clean-AppHost
	Start-Sleep -Seconds 1

	Clean-Docker
	Start-Sleep -Seconds 1

	Restore-Packages
	Start-Sleep -Seconds 1

	Build-Solution
}

function Show-Help {
	Write-Host @"
╔════════════════════════════════════════════════════════════╗
║  🐳 Aspire AppHost - Script de Gerenciamento
╚════════════════════════════════════════════════════════════╝

Comandos disponíveis:

  clean           - Limpar arquivos de build (bin/obj)
  docker-clean    - Remover containers Docker
  restore         - Restaurar pacotes NuGet
  build           - Compilar solução
  run             - Executar AppHost
  full-setup      - Fazer limpeza completa e setup
  help            - Mostrar esta mensagem

Exemplos:

  .\apphost.ps1 full-setup     # Setup completo antes de rodar
  .\apphost.ps1 run             # Rodar AppHost depois
  .\apphost.ps1 docker-clean    # Limpar containers com problemas

Status:

  AppHost: src/CustomerManager.AppHost/CustomerManager.AppHost.AppHost/
  API:     src/CustomerManager.Api/
  Dashboard: http://localhost:8000 (quando rodando)

"@
}

# Main
try {
	switch ($Action.ToLower()) {
		"clean" { Clean-AppHost }
		"docker-clean" { Clean-Docker }
		"restore" { Restore-Packages }
		"build" { Build-Solution }
		"run" { Run-AppHost }
		"full-setup" { Full-Setup; Write-Host "`n"; Run-AppHost }
		default { Show-Help }
	}
} catch {
	Print-Error "Erro: $_"
	exit 1
}
