# FraudShield

Sistema anti-fraude assíncrono baseado em eventos, composto por uma **API HTTP** e um **Worker** desacoplados via RabbitMQ.

> **Stack:** .NET 8 · ASP.NET Core · MassTransit · RabbitMQ · SQL Server · MongoDB · Redis · Docker

---

## Índice

- [Visão Geral](#visão-geral)
- [Diagramas](#diagramas)
- [Fluxo End-to-End](#fluxo-end-to-end)
- [Resiliência](#resiliência)
- [Idempotência e Deduplicação](#idempotência-e-deduplicação)
- [Observabilidade](#observabilidade)
- [Projetos](#projetos)
- [Como Rodar](#como-rodar)
- [Variáveis de Ambiente](#variáveis-de-ambiente)
- [Testes](#testes)
- [Endpoints](#endpoints)
- [Decisões Arquiteturais (ADRs)](#decisões-arquiteturais-adrs)

---

## Visão Geral

O FraudShield avalia transações financeiras em tempo real de forma assíncrona. A API e o Worker são serviços independentes — como se fossem empresas distintas — que se comunicam exclusivamente via eventos no RabbitMQ.

**Responsabilidades:**

| Componente            | Responsabilidade                                                   |
| --------------------- | ------------------------------------------------------------------ |
| `FraudShield.Api`     | Recebe a transação, persiste no SQL Server, publica evento         |
| `FraudShield.Worker`  | Consome o evento, avalia risco, persiste auditoria no MongoDB      |
| `FraudResultConsumer` | Consome o resultado e atualiza o status da transação no SQL Server |

---

## Diagramas

### Diagrama de Componentes e Containers

Visão estrutural do sistema: containers (API e Worker), seus componentes internos e as integrações com os serviços de infraestrutura.

![Diagrama de Componentes](./docs/diagrams/component-container-diagram.svg)

**Containers:**

| Container            | Tecnologia     | Responsabilidade                                             |
| -------------------- | -------------- | ------------------------------------------------------------ |
| `FraudShield.Api`    | ASP.NET Core   | Recebe requisições HTTP, orquestra persistência e publicação |
| `FraudShield.Worker` | Worker Service | Consome eventos, executa regras, persiste auditoria          |

**Componentes internos — API:**

| Componente                   | Papel                                                        |
| ---------------------------- | ------------------------------------------------------------ |
| `TransactionsController`     | Entrada HTTP, valida e roteia a requisição                   |
| `EvaluateTransactionUseCase` | Orquestra validação, persistência e publicação do evento     |
| `FraudResultConsumer`        | Consome `FraudEvaluatedResultEvent` e atualiza status no SQL |

**Componentes internos — Worker:**

| Componente             | Papel                                                     |
| ---------------------- | --------------------------------------------------------- |
| `EventValidator`       | Valida o contrato do evento recebido                      |
| `RulesEngine`          | Executa as regras `IFraudRule[]` e determina decisão/risk |
| `MongoAuditRepository` | Persiste o documento de auditoria no MongoDB              |

**Integrações de infraestrutura:**

| Serviço    | Tipo       | Uso                                                  |
| ---------- | ---------- | ---------------------------------------------------- |
| SQL Server | Relacional | Persistência transacional das transações financeiras |
| RabbitMQ   | Mensageria | Comunicação assíncrona entre API e Worker            |
| MongoDB    | NoSQL      | Auditoria de eventos processados pelo Worker         |
| Redis      | Cache      | Disponível para rate limiting e cache distribuído    |

---

### Diagrama de Sequência

Fluxo completo de ponta a ponta: da entrada da transação na API até a atualização do status final no SQL Server.

![Diagrama de Sequência](./docs/diagrams/sequence-diagram.svg)

---

## Fluxo End-to-End

```mermaid
sequenceDiagram
    participant C as Cliente
    participant A as API
    participant S as SQL Server
    participant R as RabbitMQ
    participant W as Worker
    participant M as MongoDB

    C->>A: POST /api/Transactions + X-Correlation-Id
    A->>A: Valida contrato (amount, document, merchant)
    A->>S: INSERT FinancialTransaction (status: Pending)
    S-->>A: TransactionId
    A->>R: Publish TransactionCreatedEvent
    R-->>A: ack
    A-->>C: 202 Accepted + TransactionId

    Note over R,W: Processamento assíncrono

    R->>W: Consume TransactionCreatedEvent
    W->>W: Valida contrato (EventValidator)
    W->>W: Executa RulesEngine (IFraudRule[])
    W->>M: InsertOne FraudAuditDocument
    M-->>W: audit saved
    W->>R: Publish FraudEvaluatedResultEvent

    R->>A: FraudResultConsumer
    A->>S: UPDATE status + riskLevel
```

**Decisões do RulesEngine:**

| Decision   | RiskLevel | Significado                        |
| ---------- | --------- | ---------------------------------- |
| `Approved` | Low       | Transação aprovada                 |
| `Review`   | Medium    | Sinalizada para revisão manual     |
| `Rejected` | High      | Transação bloqueada por alto risco |

---

## Resiliência

### Retry com Backoff Exponencial

O Worker e o `FraudResultConsumer` utilizam política de retry via MassTransit. Em caso de falha, a mensagem é reprocessada com intervalos crescentes:

```
Tentativa 1 → aguarda 2s
Tentativa 2 → aguarda 4s
Tentativa 3 → aguarda 8s
Esgotado   → move para DLQ (_error)
```

### Dead Letter Queue (DLQ)

Após esgotar todas as tentativas, a mensagem é movida para a fila `*_error` no RabbitMQ. O payload original é preservado integralmente — incluindo headers, `CorrelationId` e stack trace do erro.

```
antifraude-validator        → fila principal
antifraude-validator_error  → DLQ

fraud-evaluated-results        → fila de resultados
fraud-evaluated-results_error  → DLQ de resultados
```

### Fallback de Auditoria

A persistência no MongoDB é **best-effort** — falha no `SaveAsync` gera log de erro mas não interrompe o publish do resultado.

### Tabela de Resiliência

| Mecanismo                   | Onde                     | Comportamento                   |
| --------------------------- | ------------------------ | ------------------------------- |
| Retry + backoff exponencial | Worker e API (consumers) | 3 tentativas: 2s → 4s → 8s      |
| Dead Letter Queue           | RabbitMQ                 | Fila `*_error` automática       |
| Fallback auditoria          | Worker                   | Log de erro, fluxo continua     |
| Restart automático          | Docker                   | `restart: unless-stopped`       |
| Health checks               | Docker Compose           | Dependências aguardam `healthy` |

---

## Idempotência e Deduplicação

Mensagens podem ser reentregues pelo RabbitMQ em cenários de retry ou restart. A estratégia é implementada em duas camadas:

### Camada 1 — API: IdempotencyKey

Cada requisição carrega uma `IdempotencyKey` definida pelo cliente, identificando a intenção da operação de forma única.

### Camada 2 — Worker: Verificação antes de processar

```
Mensagem recebida
       ↓
CorrelationId presente? → Não → descarta + log warning
       ↓ Sim
ExistsAsync(idempotencyKey)? → Sim → descarta + log warning
       ↓ Não
Executa RulesEngine → Persiste auditoria → Publica resultado
```

### Campos de rastreabilidade

| Campo            | Tipo     | Papel                                               |
| ---------------- | -------- | --------------------------------------------------- |
| `IdempotencyKey` | `string` | Definida pelo cliente, formato livre                |
| `CorrelationId`  | `Guid`   | Gerado pela infra, rastreia o ciclo end-to-end      |
| `TransactionId`  | `Guid`   | ID do domínio, vincula auditoria à transação no SQL |

---

## Observabilidade

### Logs Estruturados

Todos os componentes utilizam `ILogger<T>` com mensagens estruturadas.

| Nível         | Evento                                                      |
| ------------- | ----------------------------------------------------------- |
| `Information` | Transação recebida, aprovada ou em revisão                  |
| `Warning`     | Transação rejeitada, contrato inválido, duplicata detectada |
| `Error`       | Falha na auditoria MongoDB, erro não tratado                |

**Campos presentes nos logs:**

```
TransactionId  → identifica a transação
CorrelationId  → rastreia a requisição end-to-end
RiskLevel      → Low / Medium / High
Decision       → Approved / Review / Rejected
```

### CorrelationId como Fio Condutor

```
X-Correlation-Id (HTTP header)
  → CorrelationId (RabbitMQ message header, via MassTransit)
    → CorrelationContext (Worker, injetado via DI)
      → FraudAuditDocument.CorrelationId (MongoDB)
        → FraudEvaluatedResultEvent.CorrelationId (resultado)
```

### Auditoria no MongoDB

```json
{
  "transactionId": "a9e04bdf-0ac4-4068-8a6e-93a030fde13a",
  "correlationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "idempotencyKey": "key-001",
  "transaction": {
    "amount": 600.0,
    "currency": "USD",
    "paymentType": "CreditCard",
    "customerDocument": "12345678900",
    "merchantName": "Loja Exemplo"
  },
  "decision": "Approved",
  "riskLevel": "Low",
  "receivedAt": "2026-03-20T10:00:00Z",
  "evaluatedAt": "2026-03-20T10:00:00.120Z"
}
```

### Extensibilidade

| Ferramenta                     | Integração                                              |
| ------------------------------ | ------------------------------------------------------- |
| **OpenTelemetry**              | MassTransit possui suporte nativo a tracing distribuído |
| **Seq / Elasticsearch**        | Logs estruturados via Serilog                           |
| **Prometheus + Grafana**       | Métricas de filas via plugin `rabbitmq_prometheus`      |
| **Azure Application Insights** | SDK para .NET com correlação automática                 |

---

## Projetos

```
FraudShield/
├── src/
│   ├── FraudShield.Api             # Endpoint HTTP, middleware de correlação
│   ├── FraudShield.Application     # Use cases, mapeamentos Mapster
│   ├── FraudShield.Domain          # Entidades, enums de domínio
│   ├── FraudShield.Infrastructure  # EF Core, repositórios, MassTransit, consumers
│   └── FraudShield.Communication   # DTOs e contratos compartilhados
├── worker/
│   └── FraudShield.Worker          # Consumer RabbitMQ, RulesEngine, auditoria MongoDB
├── docs/
│   ├── diagrams/
│   │   ├── component-container-diagram.svg   # Diagrama de componentes e containers
│   │   └── sequence-diagram.svg              # Diagrama de sequência end-to-end
│   └── adr/
│       ├── ADR-001-mensageria.md
│       ├── ADR-002-banco-de-dados.md
│       └── ADR-003-idempotencia.md
├── Dockerfile.api
├── Dockerfile.worker
└── docker-compose.yml
```

---

## Como Rodar

### Docker (recomendado)

> **Pré-requisitos:** [Docker](https://www.docker.com/get-started) e Docker Compose instalados.

```bash
# 1. Clone o repositório
git clone https://github.com/seu-usuario/fraudshield.git
cd fraudshield

# 2. Suba todos os serviços
docker-compose up --build
```

**Serviços disponíveis:**

| Serviço             | URL                    | Credenciais          |
| ------------------- | ---------------------- | -------------------- |
| API                 | http://localhost:5000  | —                    |
| RabbitMQ Management | http://localhost:15672 | guest / guest        |
| Mongo Express       | http://localhost:8081  | admin / admin123     |
| SQL Server          | localhost,1433         | sa / fraudshield@123 |

```bash
# Parar containers (mantém dados)
docker-compose down

# Reset completo (apaga volumes)
docker-compose down -v && docker-compose up --build
```

---

### Local sem Docker

> **Pré-requisitos:** .NET 8 SDK · SQL Server · RabbitMQ · MongoDB rodando localmente.

```bash
# 1. Restaurar dependências
dotnet restore

# 2. Aplicar migrations
cd src/FraudShield.Api
dotnet ef database update

# 3. Rodar a API
dotnet run

# 4. Rodar o Worker (outro terminal)
cd worker/FraudShield.Worker
dotnet run
```

---

## Variáveis de Ambiente

### API

| Variável                     | Descrição     | Exemplo Docker                                                                                               |
| ---------------------------- | ------------- | ------------------------------------------------------------------------------------------------------------ |
| `ConnectionStrings__Default` | SQL Server    | `Server=sqlserver,1433;Database=FraudShield;User Id=sa;Password=fraudshield@123;TrustServerCertificate=True` |
| `ConnectionStrings__Redis`   | Redis         | `redis:6379`                                                                                                 |
| `RabbitMq__Host`             | Host RabbitMQ | `rabbitmq`                                                                                                   |
| `RabbitMq__Username`         | Usuário       | `guest`                                                                                                      |
| `RabbitMq__Password`         | Senha         | `guest`                                                                                                      |
| `ASPNETCORE_ENVIRONMENT`     | Ambiente      | `Development`                                                                                                |

### Worker

| Variável                          | Descrição     | Exemplo Docker                           |
| --------------------------------- | ------------- | ---------------------------------------- |
| `RabbitMq__Host`                  | Host RabbitMQ | `rabbitmq`                               |
| `RabbitMq__Username`              | Usuário       | `guest`                                  |
| `RabbitMq__Password`              | Senha         | `guest`                                  |
| `MongoSettings__ConnectionString` | MongoDB       | `mongodb://admin:admin123@mongodb:27017` |
| `MongoSettings__DatabaseName`     | Database      | `fraudshield_audit`                      |
| `DOTNET_ENVIRONMENT`              | Ambiente      | `Development`                            |

> No Docker, as variáveis de ambiente do `docker-compose.yml` sobrescrevem automaticamente o `appsettings.json`.

---

## Testes

```bash
# Rodar todos os testes
dotnet test

# Com output detalhado
dotnet test --logger "console;verbosity=detailed"

# Projeto específico
dotnet test src/FraudShield.Application.Tests
dotnet test worker/FraudShield.Worker.Tests

# Com cobertura de código
dotnet test --collect:"XPlat Code Coverage"
```

---

## Endpoints

### `POST /api/Transactions`

**Headers**

| Header             | Obrigatório | Descrição                            |
| ------------------ | ----------- | ------------------------------------ |
| `Content-Type`     | Sim         | `application/json`                   |
| `X-Correlation-Id` | Não         | GUID para rastreabilidade end-to-end |

**Body**

```json
{
  "idempotencyKey": "key-001",
  "amount": 600.0,
  "createdAt": "2026-03-20T10:00:00Z",
  "currency": 1,
  "paymentType": 0,
  "customer": {
    "document": "12345678900",
    "email": "cliente@email.com",
    "ipAddress": "203.0.113.10",
    "country": "BR",
    "deviceType": 1
  },
  "merchant": {
    "name": "Loja Exemplo",
    "category": "5411",
    "country": "BR",
    "state": "SP",
    "city": "Sao Paulo"
  }
}
```

**Enums**

| Campo         | Valores                                                                 |
| ------------- | ----------------------------------------------------------------------- |
| `currency`    | `0` BRL · `1` USD · `2` EUR                                             |
| `paymentType` | `0` CreditCard · `1` DebitCard · `2` Pix · `3` Boleto                   |
| `deviceType`  | `0` Web · `1` MobileAndroid · `2` MobileIOS · `3` Desktop · `4` Unknown |

**Resposta — 202 Accepted**

```json
{
  "transactionId": "a9e04bdf-0ac4-4068-8a6e-93a030fde13a"
}
```

**Exemplo com curl**

```bash
curl -X POST http://localhost:5000/api/Transactions \
  -H "Content-Type: application/json" \
  -H "X-Correlation-Id: 3fa85f64-5717-4562-b3fc-2c963f66afa6" \
  -d '{
    "idempotencyKey": "key-001",
    "amount": 600.00,
    "createdAt": "2026-03-20T10:00:00Z",
    "currency": 1,
    "paymentType": 0,
    "customer": {
      "document": "12345678900",
      "email": "cliente@email.com",
      "ipAddress": "203.0.113.10",
      "country": "BR",
      "deviceType": 1
    },
    "merchant": {
      "name": "Loja Exemplo",
      "category": "5411",
      "country": "BR",
      "state": "SP",
      "city": "Sao Paulo"
    }
  }'
```

---

## Decisões Arquiteturais (ADRs)

| ADR                                             | Título                                             | Status    |
| ----------------------------------------------- | -------------------------------------------------- | --------- |
| [ADR-001](./docs/adr/ADR-001-mensageria.md)     | Escolha de mensageria: RabbitMQ + MassTransit      | ✅ Aceito |
| [ADR-002](./docs/adr/ADR-002-banco-de-dados.md) | Estratégia de banco de dados: SQL Server + MongoDB | ✅ Aceito |
| [ADR-003](./docs/adr/ADR-003-idempotencia.md)   | Estratégia de idempotência e deduplicação          | ✅ Aceito |
