# ADR-003 — Estratégia de idempotência e deduplicação

| Campo | Detalhe |
|---|---|
| **Data** | Março 2026 |
| **Status** | ✅ Aceito |
| **Decisores** | Tech Lead + Time de Engenharia |

---

## Contexto

Em sistemas baseados em eventos, a mesma mensagem pode ser entregue mais de uma vez pelo RabbitMQ em cenários de retry, restart de pod ou falha de ack. Sem proteção, o Worker poderia processar a mesma transação múltiplas vezes, gerando auditorias duplicadas e resultados inconsistentes.

---

## Decisão

Implementamos uma estratégia em duas camadas — uma por bounded context:

### Camada 1 — API: IdempotencyKey

- Cada requisição carrega uma `IdempotencyKey` definida pelo cliente
- A chave identifica a intenção da operação de forma única, independente de retries HTTP
- Persiste com a transação no SQL Server para rastreabilidade
- Permite que o cliente reenvie a mesma requisição com segurança

### Camada 2 — Worker: Verificação antes de processar

- `CorrelationId` gerado na API e propagado via header MassTransit end-to-end
- Worker verifica no MongoDB se a `IdempotencyKey` já foi processada antes de executar as regras
- Se já existe → descarta silenciosamente e loga o evento como duplicado
- `IdempotencyKey` indexada no MongoDB para lookup eficiente

---

## Fluxo de deduplicação no Worker

```
Mensagem recebida
       ↓
CorrelationId presente? → Não → descarta + log warning
       ↓ Sim
ExistsAsync(idempotencyKey)? → Sim → descarta + log warning
       ↓ Não
Executa RulesEngine
       ↓
InsertOne FraudAuditDocument
       ↓
Publish FraudEvaluatedResultEvent
```

---

## Campos de rastreabilidade

| Campo | Tipo | Papel |
|---|---|---|
| `IdempotencyKey` | `string` | Definida pelo cliente, formato livre, identifica a intenção |
| `CorrelationId` | `Guid` | Gerado pela infra, rastreia o ciclo de vida end-to-end |
| `TransactionId` | `Guid` | ID do domínio, vincula auditoria à transação no SQL |

---

## Consequências

- Reentregas do RabbitMQ são tratadas de forma transparente e segura
- Auditoria nunca terá documentos duplicados para a mesma transação
- `CorrelationId` permite rastrear toda a cadeia: API → Rabbit → Worker → Mongo → resultado
- Custo adicional: uma query extra ao MongoDB por mensagem processada (mitigado pelo índice)
- Índice único no MongoDB pode reforçar a garantia em nível de banco, além da verificação em código
