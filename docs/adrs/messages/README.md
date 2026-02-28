# Messages (MS)

Decisões arquiteturais sobre o sistema de mensagens (Commands, Events, Queries) que cruzam fronteiras de processo.

## ADRs

| ADR | Título | Status |
|-----|--------|--------|
| [MS-001](./MS-001-envelope-encapsulado-em-messagemetadata.md) | Envelope Encapsulado em MessageMetadata (Não Flat) | Aceita |
| [MS-002](./MS-002-schemaname-auto-computado.md) | SchemaName Auto-Computado via GetType().FullName | Aceita |
| [MS-003](./MS-003-messagemetadata-sealed-record.md) | MessageMetadata Sealed Record Sem Herança | Aceita |
| [MS-004](./MS-004-deserializacao-dois-estagios-messageenvelope.md) | Deserialização em Dois Estágios via MessageEnvelope | Aceita |
| [MS-005](./MS-005-abstract-record-hierarquia-mensagens.md) | Abstract Record Para Hierarquia de Mensagens | Aceita |
| [MS-006](./MS-006-nomenclatura-commands-events-queries.md) | Nomenclatura de Commands, Events e Queries | Aceita |
| [MS-007](./MS-007-concretos-herdam-base-tipada.md) | Concretos Herdam da Base Tipada | Aceita |
| [MS-008](./MS-008-message-models-primitivos-sem-tipos-dominio.md) | Message Models com Primitivos — Sem Tipos de Domínio | Aceita |
| [MS-009](./MS-009-snapshot-completo-aggregate-root.md) | Snapshot Completo do Aggregate Root nos Models | Aceita |
| [MS-010](./MS-010-eventos-self-contained-input-oldstate-newstate.md) | Eventos Self-Contained: Input + OldState + NewState | Aceita |

## Navegação

- [Voltar para ADRs](../README.md)
