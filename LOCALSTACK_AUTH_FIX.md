# 🔑 LOCALSTACK AUTH FIX - Guia de Resolução

## ⚠️ Problema Identificado

```
❌ License activation failed!
   Reason: No credentials were found in the environment.
   Please set LOCALSTACK_AUTH_TOKEN variable to a valid auth token.
```

---

## 🎯 Solução Rápida

### Opção 1: Usar Localstack Community (Sem Auth Token) ⭐ RECOMENDADO

No arquivo `docker-compose.yml`, modifique a configuração do LocalStack:

```yaml
localstack:
  image: localstack/localstack:latest
  container_name: localstack
  ports:
	- "4566:4566"
	- "4571:4571"
  environment:
	- SERVICES=sns,sqs,dynamodb
	- DEBUG=1
	- DATA_DIR=/tmp/localstack/data
	- DOCKER_HOST=unix:///var/run/docker.sock
	- AWS_DEFAULT_REGION=us-east-1
	- AWS_ACCESS_KEY_ID=test
	- AWS_SECRET_ACCESS_KEY=test
	- LOCALSTACK_HOST=localstack
	# ❌ REMOVA esta linha se estiver presente:
	# - LOCALSTACK_AUTH_TOKEN=seu_token_aqui
  volumes:
	- "${TMPDIR:-.}/.localstack:/tmp/localstack"
	- "/var/run/docker.sock:/var/run/docker.sock"
  networks:
	- localstack-network
```

### Opção 2: Usar Auth Token do LocalStack Pro

Se você tem um token válido:

```yaml
environment:
  - LOCALSTACK_AUTH_TOKEN=seu_token_aqui
```

### Opção 3: Desabilitar Auth Completamente

```yaml
environment:
  - DISABLE_AUTH_TOKEN_VALIDATION=1
```

---

## 📝 Arquivo `.env` Atualizado

```bash
# Docker Compose
COMPOSE_PROJECT_NAME=customermanager

# PostgreSQL
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres123
POSTGRES_DB=customermanager
POSTGRES_PORT=5432

# Redis
REDIS_PORT=6379

# LocalStack
LOCALSTACK_SERVICES=sns,sqs,dynamodb
LOCALSTACK_DOCKER_NAME=localstack
LOCALSTACK_HOSTNAME=localhost
LOCALSTACK_PORT=4566
AWS_REGION=us-east-1
AWS_ACCESS_KEY_ID=test
AWS_SECRET_ACCESS_KEY=test

# Aplicação
API_PORT=5274
ASPIRE_PORT=8000
```

---

## 🔧 Passos para Resolver

### 1️⃣ Clone/Atualize o docker-compose.yml

```powershell
# Pare os containers
docker-compose down

# Limpe volumes (se necessário)
docker-compose down -v

# Atualize a configuração (remova LOCALSTACK_AUTH_TOKEN)
# Edite docker-compose.yml conforme acima

# Reinicie
docker-compose up -d
```

### 2️⃣ Verifique o Status

```powershell
# Veja logs do LocalStack
docker-compose logs localstack

# Verifique se LocalStack está saudável
docker ps

# Teste conectividade
curl http://localhost:4566/health
```

### 3️⃣ Configure a Aplicação

No `appsettings.Development.json`:

```json
{
  "AWS": {
	"Profile": "default",
	"Region": "us-east-1",
	"SNS": {
	  "ServiceUrl": "http://localhost:4566",
	  "ContaEventosTopicArn": "arn:aws:sns:us-east-1:000000000000:conta-eventos"
	}
  }
}
```

### 4️⃣ No Código C#

```csharp
// AppServiceCollectionExtensions.cs
services.AddAWSService<IAmazonSimpleNotificationService>(new AWSOptions
{
	Credentials = new BasicAWSCredentials("test", "test"),
	Region = RegionEndpoint.USEast1
});

// Ou configure com ServiceUrl para LocalStack
var snsConfig = new AmazonSimpleNotificationServiceConfig
{
	ServiceURL = "http://localhost:4566"
};
services.AddAWSService<IAmazonSimpleNotificationService>(new AWSOptions
{
	Credentials = new BasicAWSCredentials("test", "test"),
	Region = RegionEndpoint.USEast1
});
```

---

## ✅ Checklist de Resolução

- [ ] Removeu `LOCALSTACK_AUTH_TOKEN` do docker-compose.yml
- [ ] Adicionou `AWS_ACCESS_KEY_ID=test`
- [ ] Adicionou `AWS_SECRET_ACCESS_KEY=test`
- [ ] Rodou `docker-compose down -v`
- [ ] Rodou `docker-compose up -d`
- [ ] Verificou logs: `docker-compose logs localstack`
- [ ] Testou health: `curl http://localhost:4566/health`
- [ ] Atualizou appsettings.Development.json
- [ ] Rodou aplicação: `dotnet run`
- [ ] Testou endpoint: `POST http://localhost:5274/customers`

---

## 🧪 Teste de Conectividade

```powershell
# 1. Health Check LocalStack
Invoke-WebRequest -Uri "http://localhost:4566/health" -Method GET

# 2. Listar Tópicos SNS
aws sns list-topics `
  --endpoint-url=http://localhost:4566 `
  --region=us-east-1 `
  --no-sign-request

# 3. Criar Tópico SNS (se necessário)
aws sns create-topic `
  --name conta-eventos `
  --endpoint-url=http://localhost:4566 `
  --region=us-east-1 `
  --no-sign-request
```

---

## 📊 Comparativo: Com e Sem Auth Token

| Aspecto | Com Auth Token | Sem Auth Token (Community) |
|--------|----------------|--------------------------|
| Funcionalidade | Completa (Pro) | Básica | ✅ |
| Custo | Pago | Gratuito | ✅ |
| LocalStack Community | Parcial | Completo | ✅ |
| SNS/SQS | ✅ Funciona | ✅ Funciona | ✅ |
| DynamoDB | ✅ Funciona | ✅ Funciona | ✅ |
| Recomendado para Dev | Não | Sim | ✅ |

---

## 🔐 Variáveis de Ambiente

### Para Development Local (Recomendado)

```powershell
# PowerShell
$env:AWS_ACCESS_KEY_ID = "test"
$env:AWS_SECRET_ACCESS_KEY = "test"
$env:AWS_REGION = "us-east-1"
$env:LOCALSTACK_ENDPOINT = "http://localhost:4566"
```

### Para .env

```bash
AWS_ACCESS_KEY_ID=test
AWS_SECRET_ACCESS_KEY=test
AWS_REGION=us-east-1
LOCALSTACK_ENDPOINT=http://localhost:4566
```

### No appsettings.Development.json

```json
{
  "AWS": {
	"AccessKey": "test",
	"SecretKey": "test",
	"Region": "us-east-1",
	"ServiceUrl": "http://localhost:4566"
  }
}
```

---

## 🚀 Comando de Inicialização Completa

```powershell
# 1. Parar tudo
docker-compose down -v

# 2. Limpar LocalStack
docker volume prune -f

# 3. Iniciar apenas com Community Edition
docker-compose up -d

# 4. Aguardar LocalStack ficar pronto
Start-Sleep -Seconds 5

# 5. Criar tópico SNS
docker-compose exec -T localstack awslocal sns create-topic --name conta-eventos

# 6. Verificar tópico
docker-compose exec -T localstack awslocal sns list-topics
```

---

## 🐛 Troubleshooting

### Problema: "License activation failed"
**Solução:** Remova `LOCALSTACK_AUTH_TOKEN` do docker-compose.yml

### Problema: "Connection refused on 4566"
**Solução:** Verifique se LocalStack está rodando: `docker ps | grep localstack`

### Problema: "SNS topic not found"
**Solução:** Crie o tópico manualmente ou adicione script init no docker-compose

### Problema: "AWS credentials not found"
**Solução:** Configure `AWS_ACCESS_KEY_ID` e `AWS_SECRET_ACCESS_KEY` como "test"

---

## 📚 Documentação Relacionada

- [LocalStack Community Edition](https://github.com/localstack/localstack)
- [AWS SDK .NET Configuration](https://docs.aws.amazon.com/sdk-for-net/)
- [SNS Testing com LocalStack](https://docs.localstack.cloud/services/sns/)

---

## ✨ Resumo da Solução

```
❌ ANTES (Com erro):
   image: localstack/localstack:latest
   environment:
	 - LOCALSTACK_AUTH_TOKEN=seu_token_aqui  ← ❌ Problema

✅ DEPOIS (Sem erro):
   image: localstack/localstack:latest
   environment:
	 - AWS_ACCESS_KEY_ID=test
	 - AWS_SECRET_ACCESS_KEY=test
	 - SERVICES=sns,sqs,dynamodb
	 - DEBUG=1
```

---

## 🎯 Resultado Esperado

Após aplicar a solução:

```
✅ LocalStack iniciando sem erro de auth
✅ SNS disponível em http://localhost:4566
✅ Tópicos SNS criáveis
✅ Aplicação conectando sem erros
✅ Eventos sendo publicados com sucesso
```

---

## 📞 Se Ainda Houver Problemas

1. Verifique logs: `docker-compose logs -f localstack`
2. Teste conectividade: `curl http://localhost:4566/health`
3. Verifique portas: `netstat -ano | findstr 4566`
4. Reinicie tudo: `docker-compose down -v && docker-compose up -d`

---

**Status de Resolução:** 🟢 Resolvível em 5 minutos  
**Dificuldade:** 🟢 Fácil  
**Impacto:** 🔴 Alto (necessário para SNS funcionar)

---

*Aplicar esta solução agora! ⚡*
