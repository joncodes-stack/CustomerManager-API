# CustomerManager API

API REST para gerenciamento de clientes, construída com .NET 8 seguindo os princípios de Clean Architecture e CQRS.

## 🏗️ Arquitetura

O projeto é organizado em camadas com responsabilidades bem definidas:

```
src/
├── CustomerManager.Api             # Camada de apresentação (Minimal APIs, endpoints, filtros)
├── CustomerManager.Application     # Casos de uso (Commands, Queries, Handlers, Validators, Responses)
├── CustomerManager.Domain          # Entidades e interfaces de domínio
├── CustomerManager.Infra           # Infraestrutura (EF Core, PostgreSQL, Redis, AWS SNS)
├── CustomerManager.Ioc             # Registro de dependências (IoC)
├── CustomerManager.Shared          # Recursos compartilhados
└── CustomerManager.AppHost/        # Orquestração com .NET Aspire
tests/
└── CustomerManager.UnitTests       # Testes unitários (xUnit)
```

## 🛠️ Tecnologias

| Tecnologia | Uso |
|---|---|
| .NET 8 | Plataforma |
| ASP.NET Core Minimal APIs | Exposição dos endpoints REST |
| Entity Framework Core + Npgsql | ORM e banco de dados PostgreSQL |
| Redis | Cache distribuído |
| AWS SNS (LocalStack) | Publicação de eventos de cliente |
| FluentValidation | Validação de entrada |
| Serilog + OpenTelemetry | Logs e observabilidade |
| .NET Aspire | Orquestração local dos serviços |
| xUnit | Testes unitários |

## 📋 Endpoints

Todos os endpoints estão sob o prefixo `/clientes`.

| Método | Rota | Descrição | Status de Retorno |
|---|---|---|---|
| `POST` | `/clientes` | Cria um novo cliente | `201 Created` |
| `GET` | `/clientes` | Lista clientes ativos (paginado) | `200 OK` |
| `GET` | `/clientes/{id}` | Busca cliente por ID | `200 OK` / `404 Not Found` |
| `GET` | `/clientes/cpf/{cpf}` | Busca cliente por CPF | `200 OK` / `404 Not Found` |
| `PUT` | `/clientes/{id}` | Atualiza dados do cliente | `204 No Content` |
| `DELETE` | `/clientes/{id}` | Inativa um cliente (soft delete) | `204 No Content` |

### Modelo do Cliente

```json
{
  "id": "guid",
  "cardHolderName": "string",
  "cpf": "string",
  "status": true
}
```

## 🚀 Como executar

### Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/) (para PostgreSQL, Redis e LocalStack via .NET Aspire)
- Token do LocalStack Pro (para emulação do AWS SNS) — defina a variável de ambiente `LOCALSTACK_AUTH_TOKEN`

### Com .NET Aspire (recomendado)

O AppHost orquestra automaticamente o PostgreSQL, Redis e LocalStack:

```bash
cd src/CustomerManager.AppHost/CustomerManager.AppHost.AppHost
dotnet run
```

O Aspire irá subir os containers e configurar as connection strings automaticamente. As migrations são aplicadas automaticamente na inicialização.

### Com Docker (somente a API)

```bash
docker build -f src/CustomerManager.Api/Dockerfile -t customer-manager-api .
docker run -p 8080:8080 customer-manager-api
```

> Certifique-se de configurar as variáveis de ambiente para PostgreSQL, Redis e SNS antes de executar o container.

### Variáveis de configuração

| Chave | Descrição | Padrão |
|---|---|---|
| `ConnectionStrings:Default` | Connection string do PostgreSQL | — |
| `ConnectionStrings:Redis` | Connection string do Redis | `localhost:6379` |
| `AWS:SNS:ServiceUrl` | URL do SNS (LocalStack) | `http://localhost:4566` |
| `AWS:SNS:ContaEventosTopicArn` | ARN do tópico SNS | — |

## ✅ Testes

O projeto conta com testes unitários organizados por camada:

```
tests/CustomerManager.UnitTests/
├── Application/
│   ├── Handlers/    # Testes dos handlers de Commands e Queries
│   └── Validators/  # Testes dos validators FluentValidation
├── Domain/
│   └── Entities/    # Testes das entidades de domínio
├── Infra/
│   └── Repositories/ # Testes dos repositórios
└── Shared/
	├── Builders/    # Builders para criação de dados de teste
	└── Fixtures/    # Fixtures compartilhadas (FakeDbContext, MockFactory)
```

### Executar os testes

```bash
dotnet test tests/CustomerManager.UnitTests/CustomerManager.UnitTests.csproj
```

## 📐 Padrões e boas práticas

- **Clean Architecture**: separação clara entre domínio, aplicação e infraestrutura.
- **CQRS**: commands e queries separados com seus respectivos handlers.
- **Repository Pattern**: abstração de acesso a dados via `IBaseRepository` e `ICustomerRepository`.
- **Soft Delete**: a exclusão inativa o cliente (`Status = false`) sem remover o registro.
- **Retry Policy**: conexão com o PostgreSQL configurada com retry automático em falhas.
- **Event-driven**: publicação de eventos de cliente via AWS SNS após operações de escrita.
- **Validação**: FluentValidation aplicada via `IEndpointFilter` antes de atingir os handlers.
- **Migrations automáticas**: aplicadas na inicialização da aplicação.
