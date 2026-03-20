# ADR-001 — Escolha de mensageria: RabbitMQ + MassTransit

| Campo | Detalhe |
|---|---|
| **Data** | Março 2026 |
| **Status** | ✅ Aceito |
| **Decisores** | Tech Lead + Time de Engenharia |

---

## Contexto

O FraudShield precisa desacoplar completamente a API (que recebe a transação) do Worker (que avalia o risco). Essa separação exige um mecanismo de mensageria confiável, com suporte a retry, dead-letter e serialização flexível.

---

## Alternativas avaliadas

| Alternativa | Avaliação |
|---|---|
| **RabbitMQ + MassTransit** ✅ | Time com domínio consolidado. Setup rápido. MassTransit abstrai retry, DLQ e serialização nativamente. |
| Apache Kafka | Alta throughput para volumes massivos, mas curva de aprendizado elevada e overhead operacional maior para o escopo do case. |
| Azure Service Bus | Opção gerenciada e integrada ao Azure, mas adiciona dependência de cloud e complexidade de configuração local. |

---

## Decisão

Adotamos **RabbitMQ** como broker e **MassTransit** como abstração de publicação e consumo.

**Fatores determinantes:**

- **Conhecimento do time** — a equipe já possuía domínio em RabbitMQ, reduzindo riscos de entrega no prazo. Kafka seria avaliado em um cenário de escala maior (milhões de eventos/dia).
- **MassTransit** entrega nativamente retry com backoff exponencial, dead-letter queue (`_error`) e propagação de `CorrelationId`
- Serialização Raw JSON configurável, compatível com os contratos do domínio
- Setup local simples via Docker, facilitando onboarding

> Kafka seria a escolha natural em cenário de alto volume. Para o escopo atual, RabbitMQ entrega o mesmo resultado com menor complexidade operacional.

---

## Consequências

- Retry automático com backoff exponencial: 3 tentativas (2s → 4s → 8s)
- Mensagens que esgotam retries são movidas para fila `_error` (DLQ)
- `CorrelationId` propagado automaticamente em todos os publishes via MassTransit
- Troca futura por Kafka requer reimplementar apenas os adaptadores de infraestrutura (`IMessageBus`), sem impacto no domínio
