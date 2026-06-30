# 🔧 Aspire Hosting - Resolução de Conflito de Versões

## 📋 Problema Identificado

O seu AppHost tinha um **conflito de versionamento** que impedia a atualização:

```xml
<!-- ❌ PROBLEMA: Versões incompatíveis -->
<Project Sdk="Aspire.AppHost.Sdk/13.2.4">
  ...
  <PackageReference Include="Aspire.Hosting.PostgreSQL" Version="13.4.6" />
  <PackageReference Include="Aspire.Hosting.Redis" Version="13.4.6" />
</Project>
```

**O SDK era 13.2.4 mas os pacotes eram 13.4.6** - isso causava um bloqueio!

---

## ✅ Solução Aplicada

O arquivo foi atualizado para **sincronizar todas as versões para 13.4.6**:

```xml
<!-- ✅ CORRETO: Versões sincronizadas -->
<Project Sdk="Aspire.AppHost.Sdk/13.4.6">
  ...
  <PackageReference Include="Aspire.Hosting.PostgreSQL" Version="13.4.6" />
  <PackageReference Include="Aspire.Hosting.Redis" Version="13.4.6" />
</Project>
```

---

## 📁 Arquivo Modificado

`src/CustomerManager.AppHost/CustomerManager.AppHost.AppHost/CustomerManager.AppHost.csproj`

---

## 🚀 Próximos Passos

### 1. Limpar cache e dependências
```powershell
cd "C:\source\Projetos Pessoais\Apis\CustomerManager-API"
dotnet clean
rm -r src/CustomerManager.AppHost/CustomerManager.AppHost.AppHost/bin
rm -r src/CustomerManager.AppHost/CustomerManager.AppHost.AppHost/obj
```

### 2. Restaurar pacotes
```powershell
dotnet restore
```

### 3. Executar o AppHost
```powershell
dotnet run --project src/CustomerManager.AppHost/CustomerManager.AppHost.AppHost/CustomerManager.AppHost.csproj
```

---

## ℹ️ O Que é Aspire Hosting

O **Aspire Hosting** é um componente do .NET Aspire que:
- ✅ Gerencia containers Docker (PostgreSQL, Redis, LocalStack)
- ✅ Configura service discovery
- ✅ Facilita desenvolvimento local com múltiplos serviços
- ✅ Oferece observabilidade com OpenTelemetry

---

## 🔍 Arquivos na Solução Aspire

```
CustomerManager.AppHost/
├── CustomerManager.AppHost.AppHost/
│   ├── AppHost.cs                          # Configuração dos containers
│   └── CustomerManager.AppHost.csproj      # ✅ Corrigido!
│
└── CustomerManager.AppHost.ServiceDefaults/
	├── Extensions.cs
	└── CustomerManager.AppHost.ServiceDefaults.csproj
```

---

## 📊 Componentes do AppHost.cs

```csharp
// PostgreSQL - Banco de dados principal
var postgres = builder.AddPostgres("Postgres", port: 5432)
	.WithPgAdmin(...)
	.AddDatabase("Default", "customer-manager-db");

// Redis - Cache
var redis = builder.AddRedis("redis");

// LocalStack - AWS Local (SNS)
var localstack = builder.AddContainer("localstack", "localstack/localstack")
	.WithEnvironment("SERVICES", "sns");

// API - Sua aplicação
builder.AddProject<Projects.CustomerManager_Api>("api")
	.WithReference(postgres)
	.WithReference(redis)
	.WaitFor(postgres)
	.WaitFor(redis)
	.WaitFor(localstack);
```

---

## 🆘 Se Ainda Tiver Problemas

### Erro: "Aspire SDK não encontrado"
```powershell
# Atualize o .NET SDK
dotnet --version  # Deve ser 8.0.x ou superior
```

### Erro: "Container já em uso"
```powershell
# Limpe containers existentes
docker ps -a
docker rm -f app-postgres app-redis app-localstack
```

### Erro ao restaurar pacotes
```powershell
# Limpe cache do NuGet
dotnet nuget locals all --clear
dotnet restore
```

---

## ✨ Status Atual

✅ **Build:** Sucesso!  
✅ **Versionamento:** Sincronizado  
✅ **Pronto para rodar:** Sim!

Execute `dotnet run` no AppHost e aproveite seu ambiente local completo! 🎉

---

**Arquivo alterado:** `CustomerManager.AppHost.csproj`  
**Versão Aspire:** 13.4.6  
**Status:** ✅ Resolvido
