# ADR-002 — Estratégia de banco de dados: SQL Server + MongoDB

| Campo | Detalhe |
|---|---|
| **Data** | Março 2026 |
| **Status** | ✅ Aceito |
| **Decisores** | Tech Lead + Time de Engenharia |

---

## Contexto

O sistema possui dois bounded contexts com necessidades distintas de persistência. A **API** gerencia transações financeiras com integridade relacional. O **Worker** processa e persiste eventos de auditoria com schema flexível e alto volume de escrita.

---

## Alternativas avaliadas

| Cenário | Avaliação |
|---|---|
| **SQL Server (API) + MongoDB (Worker)** ✅ | Cada contexto usa o banco mais adequado. SQL para consistência transacional, MongoDB para flexibilidade e volume. |
| Apenas SQL Server | Simplicidade operacional, mas força schema rígido para dados de auditoria que variam por tipo de evento e regras disparadas. |
| Apenas MongoDB | Flexível, mas perde garantias ACID necessárias para transações financeiras. |

---

## Decisão

Adotamos **SQL Server na API** e **MongoDB no Worker**, cada um justificado pelo seu contexto:

### SQL Server — API

- Transações financeiras exigem consistência ACID e integridade referencial
- Schema previsível e estável: `Amount`, `Status`, `RiskLevel`, `Customer`, `Merchant`
- EF Core + migrations garantem versionamento e rastreabilidade do schema
- Queries relacionais para relatórios e reconciliação de status

### MongoDB — Worker (auditoria)

- Auditoria persiste o snapshot completo do evento recebido (JSON livre)
- Schema flexível permite evoluir o documento sem migrations
- Alto volume de escritas: inserção por evento processado, sem updates
- Indexação por `TransactionId`, `CorrelationId` e `IdempotencyKey` para queries eficientes
- Mongo Express disponível para inspeção visual dos documentos

---

## Consequências

- Worker é totalmente independente da API: não compartilha banco nem schema
- Auditoria preserva o dado exatamente como recebido, sem perda de contexto
- Operações de leitura na auditoria são simples e diretas por índice
- Custo operacional adicional: dois bancos para monitorar e manter
