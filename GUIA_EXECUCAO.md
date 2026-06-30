# 🚀 Guia de Execução - CustomerManager-API

## ✅ Pré-requisitos

- ✅ .NET 8 SDK instalado
- ✅ Docker & Docker Compose (para PostgreSQL, Redis, LocalStack)
- ✅ Visual Studio Community 2026 ou VS Code
- ✅ Git

---

## 🐳 1. Iniciar Serviços Docker

### Verificar se Docker está rodando
```powershell
docker --version
docker-compose --version
```

### Iniciar todos os serviços
```powershell
cd docker
docker-compose up -d
docker-compose ps
```

Você deve ver:
- ✅ `app-postgres` (5432)
- ✅ `app-redis` (6379)
- ✅ `app-localstack` (4566)

---

## 📋 2. Validar Configurações

### Executar validação de DI
```powershell
.\validate-di.ps1
```

Deve mostrar todos os ✅ verdes

### Testar conexão com PostgreSQL
```powershell
psql -h localhost -U postgres -d customer-manager-db -c "SELECT 1;"
```

### Testar conexão com Redis
```bash
redis-cli ping
# Esperado: PONG
```

### Testar LocalStack
```bash
curl http://localhost:4566/_localstack/health
```

---

## 🏃 3. Executar a Aplicação

### Opção A: Visual Studio (Recomendado)
1. Abra `CustomerManager.slnx` no Visual Studio
2. Defina como projeto de inicialização: `CustomerManager.Api`
3. Pressione `F5` (Debug) ou `Ctrl+F5` (Release)
4. Acesse http://localhost:5274 (ou porta informada)

### Opção B: Visual Studio Code
```bash
cd src/CustomerManager.Api
dotnet run
```

### Opção C: AppHost (Aspire)
```bash
dotnet run --project src/CustomerManager.AppHost/CustomerManager.AppHost.AppHost/CustomerManager.AppHost.csproj
```

Acesse o Dashboard em http://localhost:8000

### Opção D: Command Line
```bash
dotnet build
dotnet run --project src/CustomerManager.Api/CustomerManager.Api.csproj
```

---

## 🌐 4. Acessar a Aplicação

### Swagger UI
```
http://localhost:5274/swagger/index.html
```

### Endpoints Disponíveis

#### Create Customer
```bash
curl -X POST http://localhost:5274/customers \
  -H "Content-Type: application/json" \
  -d '{
	"cardHolderName": "João Silva",
	"cpf": "12345678901"
  }'
```

#### Get All Customers
```bash
curl http://localhost:5274/customers
```

#### Get Customer by CPF (com cache Redis)
```bash
curl http://localhost:5274/customers/cpf/12345678901
```

#### Update Customer
```bash
curl -X PATCH http://localhost:5274/customers/{id} \
  -H "Content-Type: application/json" \
  -d '{
	"cardHolderName": "João Silva Updated",
	"cpf": "12345678902"
  }'
```

#### Delete Customer
```bash
curl -X DELETE http://localhost:5274/customers/{id}
```

---

## 📊 5. Monitorar Aplicação

### Logs em Tempo Real
```bash
dotnet run --project src/CustomerManager.Api/CustomerManager.Api.csproj
```

### Verificar Cache Redis
```bash
redis-cli
> KEYS *
> GET customer:cpf:12345678901:2024-01-15
```

### Verificar SNS Events (LocalStack)
```bash
docker-compose exec localstack awslocal sns list-topics --region us-east-1
```

### Health Check
```bash
curl http://localhost:5274/health
```

---

## 🧪 6. Testes

### Executar testes unitários
```bash
dotnet test
```

### Executar teste específico
```bash
dotnet test --filter "TestClassName"
```

---

## 🛑 7. Parar Aplicação

### Parar Docker Compose
```bash
cd docker
docker-compose down
```

### Parar e remover volumes (perder dados)
```bash
cd docker
docker-compose down -v
```

---

## 📝 8. Troubleshooting

### Erro: "Port 5432 already in use"
```bash
# Matar processo na porta
netstat -ano | findstr :5432
taskkill /PID <PID> /F

# Ou usar porta diferente no docker-compose.yml
```

### Erro: "Unable to resolve service"
```bash
# Executar validação
.\validate-di.ps1

# E verificar DI_RESOLUTION_FIX.md
```

### Erro: "Connection refused"
```bash
# Verifique se Docker está rodando
docker ps

# Reinicie Docker Compose
cd docker
docker-compose restart
```

### Erro: "Database does not exist"
```bash
# PostgreSQL criará automaticamente
# Se não, execute migrations
dotnet ef database update --project src/CustomerManager.Infra/CustomerManager.Infra.csproj
```

---

## 📊 Arquitetura da Aplicação

```
┌─────────────────────────────────────────────────────────────┐
│                   HTTP Client (Swagger)                    │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  CustomerManager.Api                                        │
│  ├─ Endpoints (REST)                                        │
│  └─ Program.cs (Configuration)                              │
│         ↓                                                   │
│  CustomerManager.Application                                │
│  ├─ Handlers (Business Logic)                               │
│  ├─ Commands & Queries                                      │
│  └─ Validators                                              │
│         ↓                                                   │
│  CustomerManager.Domain                                     │
│  ├─ Entities (Models)                                       │
│  ├─ Interfaces                                              │
│  └─ Business Rules                                          │
│         ↓                                                   │
│  CustomerManager.Infra                                      │
│  ├─ Database (EF Core + PostgreSQL)                         │
│  ├─ Repositories                                            │
│  ├─ Messaging (SNS)                                         │
│  └─ Caching (Redis)                                         │
│         ↓                                                   │
│  External Services                                          │
│  ├─ PostgreSQL (5432)                                       │
│  ├─ Redis (6379)                                            │
│  └─ LocalStack/SNS (4566)                                   │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## 🔐 Credenciais Padrão

| Serviço | Usuário | Senha | Banco |
|---------|---------|-------|-------|
| PostgreSQL | postgres | postgres | customer-manager-db |
| Redis | (nenhum) | (nenhum) | - |
| LocalStack | - | - | - |
| API | (sem auth) | (sem auth) | - |

---

## 📈 Performance

### Cache Redis
- Ativado para: GetCustomerByCpf
- TTL: Até fim do dia (meia-noite)
- Invalidação: On Update/Delete

### Database
- Connection Pool: Automático
- Retry Policy: 5 tentativas, 10s delay
- Migrations: Automáticas no startup (Dev)

---

## 🔍 Monitoramento

### AppHost Dashboard (se usar Aspire)
```
http://localhost:8000
```

### Application Logs
```
Sink: Console + File (em logs/)
Level: Information em Dev, Warning em Prod
```

---

## 📚 Documentação Complementar

- **DI_RESOLUTION_FIX.md** - Detalhes técnicos de DI
- **RESUMO_DI_FINAL.md** - Resumo das correções
- **docker/README.md** - Documentação Docker Compose
- **ASPIRE_HOSTING_FIX.md** - Problema Aspire resolvido

---

## ✅ Quick Checklist

- [ ] Docker rodando (`docker ps`)
- [ ] PostgreSQL acessível (`psql ...`)
- [ ] Redis acessível (`redis-cli ping`)
- [ ] LocalStack acessível (`curl http://localhost:4566/...`)
- [ ] Validação DI passando (`.\validate-di.ps1`)
- [ ] Build sem erros (`dotnet build`)
- [ ] Aplicação iniciando (`dotnet run`)
- [ ] Swagger acessível (`http://localhost:5274/swagger`)
- [ ] Endpoint respondendo (`curl http://localhost:5274/customers`)

---

## 🎉 Pronto!

Sua aplicação está pronta para ser usada! 🚀

**Próximos passos:**
1. Explore os endpoints no Swagger
2. Crie alguns customers
3. Verifique o cache em Redis
4. Monitore os eventos SNS
5. Customize conforme sua necessidade

---

**Precisa de ajuda?** Consulte os arquivos de documentação ou execute os scripts de validação!

**Status:** ✅ **PRONTO PARA DESENVOLVIMENTO**
