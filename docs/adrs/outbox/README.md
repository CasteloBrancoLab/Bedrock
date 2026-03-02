# Outbox ADRs

Decisoes arquiteturais sobre o padrao Transactional Outbox e sua implementacao no Bedrock.

## ADRs

| ADR | Titulo | Status | Rule |
|-----|--------|--------|------|
| [OB-001](./OB-001-outboxentry-record-infraestrutura.md) | OutboxEntry como Record de Infraestrutura | Aceita | — |
| [OB-002](./OB-002-payload-bytes-agnostico-formato.md) | Payload Format-Agnostico com byte[] e ContentType | Aceita | — |
| [OB-003](./OB-003-lease-pattern-for-update-skip-locked.md) | Lease Pattern com FOR UPDATE SKIP LOCKED | Aceita | — |
| [OB-004](./OB-004-lazy-sql-initialization.md) | Inicializacao Lazy de SQL no Repositorio | Aceita | — |
| [OB-005](./OB-005-dead-lettering-automatico-maxretries.md) | Dead-Lettering Automatico por MaxRetries | Aceita | — |
| [OB-006](./OB-006-repositorio-tres-camadas.md) | Repositorio Outbox em Tres Camadas | Aceita | — |
| [OB-007](./OB-007-desserializacao-duas-fases.md) | Desserializacao em Duas Fases | Aceita | — |
| [OB-008](./OB-008-composicao-sobre-heranca-outboxwriter.md) | Composicao sobre Heranca para OutboxWriter do BC | Aceita | — |
| [OB-009](./OB-009-naming-convention-tabela-outbox-por-bc.md) | Naming Convention de Tabelas Outbox por BC | Aceita | — |
| [OB-010](./OB-010-indices-parciais-estrategicos.md) | Indices Parciais Estrategicos para Outbox | Aceita | — |
| [OB-011](./OB-011-marker-interfaces-outbox-por-bc.md) | Marker Interfaces para Outbox por BC | Aceita | — |
| [OB-012](./OB-012-telemetria-monitorizacao-outbox.md) | Telemetria e Monitorizacao do Outbox | Aceita | — |
| [OB-013](./OB-013-background-worker-processamento.md) | Estrategia de Background Worker para Processamento | Aceita | — |

## Escopo

- **OB-***: Regras que definem padroes obrigatorios para o Transactional Outbox — persistencia, concorrencia, serializacao, processamento e integracao com bounded contexts.

## Navegacao

- [Voltar para ADRs](../README.md)
- [Infrastructure ADRs](../infrastructure/README.md)
- [Messages ADRs](../messages/README.md)
- [PostgreSQL ADRs](../postgresql/README.md)
