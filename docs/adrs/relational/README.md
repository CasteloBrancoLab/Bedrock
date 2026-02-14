# Relational ADRs

Decisoes arquiteturais sobre padroes especificos de bancos de dados relacionais (mappers, SQL generation, data models).

## ADRs

| ADR | Titulo | Status | Rule |
|-----|--------|--------|------|
| [RL-001](./RL-001-mapper-herda-datamodelmapperbase.md) | Mapper Deve Herdar DataModelMapperBase | Aceita | RL001 |
| [RL-002](./RL-002-mapper-configurar-maptable.md) | ConfigureInternal do Mapper Deve Chamar MapTable | Aceita | RL002 |
| [RL-003](./RL-003-proibir-sql-fora-de-mapper.md) | Proibir SQL Literal Fora de Mappers | Aceita | RL003 |
| [RL-004](./RL-004-datamodel-propriedades-primitivas.md) | DataModel Deve Ter Apenas Propriedades Primitivas | Aceita | RL004 |

## Escopo

- **RL-***: Regras que definem padroes obrigatorios para mapeamento objeto-relacional, geracao de SQL e estrutura de data models.

## Navegacao

- [Voltar para ADRs](../README.md)
- [Infrastructure ADRs](../infrastructure/README.md)
- [PostgreSQL ADRs](../postgresql/README.md)
