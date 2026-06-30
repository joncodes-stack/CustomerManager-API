# 🔧 Resolução de Problemas de Injeção de Dependência (DI)

## ❌ Problema Original

```
Error while validating the service descriptor 'ServiceType: CustomerManager.Application.Interfaces.ICreateCustomerHandler':
Unable to resolve service for type 'CustomerManager.Application.Interfaces.ICustomerEventPublisher' 
while attempting to activate 'CustomerManager.Application.Handlers.CreateCustomerHandler'.

Error while validating the service descriptor 'ServiceType: CustomerManager.Application.Handlers.GetCustomerByCpfHandler':
Unable to resolve service for type 'Microsoft.Extensions.Caching.Distributed.IDistributedCache' 
while attempting to activate 'CustomerManager.Application.Handlers.GetCustomerByCpfHandler'.
```

**Causa:** Faltavam registros no DI Container e dependências não estavam configuradas.

---

## ✅ Soluções Implementadas

### 1. **Resolveu Referência Cíclica com ICustomerEventPublisher**

#### Problema
- `Application` referenciava `Infra`
- `Infra` necessitava implementar `ICustomerEventPublisher`
- Isso criaria um ciclo: `Infra → Application → Infra`

#### Solução
- Moveu `ICustomerEventPublisher` para `Domain.Interfaces.Services`
- `Domain` não tem dependências, resolvendo o ciclo

**Antes:**
```
Application.Interfaces.ICustomerEventPublisher
├── Problema: Application referencia Infra
└── Se Infra implementasse, criaria ciclo
```

**Depois:**
```
Domain.Interfaces.Services.ICustomerEventPublisher ✅
├── Domain sem dependências
├── Application depende de Domain ✅
└── Infra depende de Domain ✅
```

---

### 2. **Registrou ICustomerEventPublisher no DI Container**

**Arquivo:** `src/CustomerManager.Ioc/AppServiceCollectionExtensions.cs`

```csharp
// AWS SNS Configuration
services.AddAWSService<IAmazonSimpleNotificationService>();

// Event Publisher
services.AddScoped<ICustomerEventPublisher, CustomerEventPublisher>();
```

---

### 3. **Configurou IDistributedCache (Redis)**

**Arquivo:** `src/CustomerManager.Ioc/AppServiceCollectionExtensions.cs`

```csharp
// Distributed Cache (Redis)
services.AddStackExchangeRedisCache(options =>
{
	options.Configuration = configuration.GetConnectionString("Redis") ?? "localhost:6379";
});
```

---

### 4. **Adicionou NuGet Packages Necessários**

**Arquivo:** `src/CustomerManager.Ioc/CustomerManager.Ioc.csproj`

```xml
<ItemGroup>
	<PackageReference Include="AWSSDK.Extensions.NETCore.Setup" Version="3.7.300" />
	<PackageReference Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="8.0.8" />
</ItemGroup>
```

---

### 5. **Configurou ConnectionStrings**

**Arquivo:** `src/CustomerManager.Api/appsettings.Development.json`

```json
{
  "ConnectionStrings": {
	"Default": "Server=localhost;Port=5432;Database=customer-manager-db;User Id=postgres;Password=postgres;",
	"Redis": "localhost:6379"
  },
  "AWS": {
	"SNS": {
	  "ContaEventosTopicArn": "arn:aws:sns:us-east-1:000000000000:daily-spending-events"
	}
  }
}
```

---

### 6. **Atualizou Handlers para Usar Novo Modelo de Mensagem**

#### Antes
```csharp
await _eventPublisher.PublicarAsync(new CustomerEvent
{
	TipoEvento = "ContaCriada",
	CustomerId = user.Id.ToString(),
	OcorridoEm = DateTime.UtcNow
});
```

#### Depois
```csharp
await _eventPublisher.PublicarAsync(new CustomerEventMessage
{
	TipoEvento = "ContaCriada",
	CustomerId = user.Id.ToString(),
	DataEvento = DateTime.UtcNow
});
```

**Atualizações realizadas em:**
- ✅ `CreateCustomerHandler.cs`
- ✅ `UpdateCustomerHandler.cs`
- ✅ `DeleteCustomerHandler.cs`

---

## 📁 Arquivos Modificados

| Arquivo | Ação | Detalhes |
|---------|------|----------|
| `AppServiceCollectionExtensions.cs` | ✏️ Modificado | Adicionado registros de DI para AWS SNS, Redis, ICustomerEventPublisher |
| `appsettings.Development.json` | ✏️ Modificado | Adicionado ConnectionStrings para PostgreSQL, Redis e AWS |
| `ICustomerEventPublisher.cs` | ➕ Criado | Movido para `Domain/Interfaces/Services/` |
| `CustomerEventPublisher.cs` | ✏️ Modificado | Implementa interface do Domain (sem ciclo) |
| `CreateCustomerHandler.cs` | ✏️ Modificado | Usa novo using e modelo de mensagem |
| `UpdateCustomerHandler.cs` | ✏️ Modificado | Usa novo using e modelo de mensagem |
| `DeleteCustomerHandler.cs` | ✏️ Modificado | Usa novo using e modelo de mensagem |
| `CustomerManager.Ioc.csproj` | ✏️ Modificado | Adicionado pacotes AWS e Redis |
| `ICustomerEventPublisher.cs` | 🗑️ Removido | Versão antiga do Application.Interfaces |

---

## 🏗️ Arquitetura de Dependências - Antes vs Depois

### ❌ Antes (Com Problema)

```
Application
	├─> ICustomerEventPublisher (não registrado no DI!)
	└─> Infra
		└─> IDistributedCache (não registrado no DI!)

Resultado: NullReferenceException no runtime ❌
```

### ✅ Depois (Resolvido)

```
Domain
	└─> ICustomerEventPublisher ✅
	└─> ICustomerService ✅

Application
	├─> Domain ✅
	└─> Infra
		└─> Domain ✅

Infra
	├─> Domain ✅
	└─> Amazon SNS SDK ✅

IoC (DI Container)
	├─> ICustomerEventPublisher → CustomerEventPublisher ✅
	├─> IDistributedCache → RedisCache ✅
	├─> IAmazonSimpleNotificationService → AWS SNS ✅
	└─> Handlers registrados ✅

Resultado: Resolução completa em runtime ✅
```

---

## 🔐 Modelo de Mensagem

### CustomerEventMessage (Domain)

```csharp
public class CustomerEventMessage
{
	public string TipoEvento { get; set; }
	public string CustomerId { get; set; }
	public DateTime DataEvento { get; set; }
	public string? Detalhes { get; set; }
}
```

**Usada por:**
- ✅ `CreateCustomerHandler` - Evento "ContaCriada"
- ✅ `UpdateCustomerHandler` - Evento "ContaAtualizada"
- ✅ `DeleteCustomerHandler` - Evento "ContaDeletada"

---

## 🚀 Dependências Agora Registradas

| Interface | Implementação | Lifetime | Propósito |
|-----------|---------------|----------|-----------|
| `ICustomerEventPublisher` | `CustomerEventPublisher` | Scoped | Publicar eventos SNS |
| `IDistributedCache` | Redis (StackExchange) | Scoped | Cache distribuído |
| `IAmazonSimpleNotificationService` | AWS SDK | Scoped | Serviço SNS da AWS |
| `ICreateCustomerHandler` | `CreateCustomerHandler` | Scoped | Handler de criação |
| `IGetAllCustomerHandler` | `GetAllCustomersHandler` | Scoped | Handler de listagem |
| `IGetCustomerByCPFHandler` | `GetCustomerByCpfHandler` | Scoped | Handler de busca por CPF |
| `IUpdateCustomerHandler` | `UpdateCustomerHandler` | Scoped | Handler de atualização |
| `IDeleteCustomerHandler` | `DeleteCustomerHandler` | Scoped | Handler de deleção |
| `ICustomerRepository` | `CustomerRepository` | Scoped | Repositório de clientes |

---

## 🔌 Configuração do Redis

O Redis é usado para cache distribuído via `IDistributedCache`:

```csharp
// GetCustomerByCpfHandler usa cache
var chave = $"customer:cpf:{query.Cpf}:{DateTime.UtcNow:yyyy-MM-dd}";
var cached = await _cache.GetStringAsync(chave);
if (cached is not null)
	return JsonSerializer.Deserialize<GetCustomerByCpfResponse>(cached);

// Depois de buscar do banco, cacheia
await _cache.SetStringAsync(chave, JsonSerializer.Serialize(response),
	new DistributedCacheEntryOptions
	{
		AbsoluteExpirationRelativeToNow = expiracao
	});
```

---

## 📊 Chain de Resolução no Runtime

```
HTTP Request
	↓
CustomerManagerEndpoints
	↓
Handler (ex: CreateCustomerHandler)
	↓
DI Container resolve:
	├─> ILogger<CreateCustomerHandler> ✅
	├─> ICustomerRepository ✅
	│   └─> CustomerContext ✅
	├─> ICustomerEventPublisher ✅
	│   └─> IAmazonSimpleNotificationService ✅
	└─> ICustomerEventPublisher.PublicarAsync()
		└─> SNS (Publicação de evento)

Resultado: ✅ Sucesso!
```

---

## ✅ Status Atual

**Build:** ✅ Sucesso!  
**Compilação:** ✅ Sem erros  
**DI Container:** ✅ Totalmente configurado  
**Pronto para:** ✅ Executar aplicação  

---

## 🧪 Testando a Configuração

### 1. Verificar se handlers estão sendo injetados

```csharp
// No seu endpoint, o handler será automaticamente injetado
[HttpPost("/customers")]
public async Task<IResult> Create(
	CreateCustomerCommand command,
	[FromServices] ICreateCustomerHandler handler) // ✅ Resolvido!
{
	var result = await handler.Handle(command);
	return Results.Ok(result);
}
```

### 2. Verificar se Redis está funcionando

```bash
# Terminal
redis-cli ping
# Resposta esperada: PONG
```

### 3. Verificar se SNS está acessível (LocalStack)

```bash
# Verificar LocalStack
curl http://localhost:4566/_localstack/health

# Ou dentro da aplicação, verifique os logs de publicação
```

---

## 🐛 Se Ainda Houver Problemas

### Erro: "Unable to resolve service for type..."

```csharp
// Significa que faltou registrar no IoC
// Verifique AppServiceCollectionExtensions.cs
services.AddScoped<ISeuServico, SuaImplementacao>();
```

### Erro: "Redis connection refused"

```bash
# Verifique se Redis está rodando
docker ps | grep redis

# Ou inicie via Docker Compose
cd docker
docker-compose up -d redis
```

### Erro: "TopicArn is null"

```json
// Verifique appsettings.Development.json
{
  "AWS": {
	"SNS": {
	  "ContaEventosTopicArn": "arn:aws:sns:us-east-1:000000000000:daily-spending-events"
	}
  }
}
```

---

## 📚 Referências de Código

### ICustomerEventPublisher (Domain)
```csharp
namespace CustomerManager.Domain.Interfaces.Services
{
	public interface ICustomerEventPublisher
	{
		Task PublicarAsync(CustomerEventMessage evento);
	}
}
```

### Registros no IoC
```csharp
services.AddAWSService<IAmazonSimpleNotificationService>();
services.AddStackExchangeRedisCache(options => 
	options.Configuration = configuration.GetConnectionString("Redis"));
services.AddScoped<ICustomerEventPublisher, CustomerEventPublisher>();
```

---

## 🎉 Conclusão

Todos os problemas de DI foram resolvidos:
- ✅ Ciclo de referência eliminado
- ✅ Cache distribuído configurado
- ✅ SNS da AWS integrado
- ✅ Todos os handlers registrados
- ✅ Build com sucesso
- ✅ Pronto para executar!

**Próximo passo:** Execute `dotnet run` e teste a aplicação! 🚀

---

**Arquivo:** `DI_RESOLUTION_FIX.md`  
**Status:** ✅ **RESOLVIDO**  
**Data:** 2024
