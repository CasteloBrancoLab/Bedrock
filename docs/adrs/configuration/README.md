# Configuration ADRs

Decisoes arquiteturais sobre o BuildingBlock de Configuration: pipeline de handlers, gerenciamento tipado de configuracoes, estrategias de caching e escopo de aplicacao.

## ADRs

### Fundamentos

| ADR | Titulo | Status |
|-----|--------|--------|
| [CF-001](./CF-001-manager-herda-configurationmanagerbase.md) | ConfigurationManager Deve Herdar ConfigurationManagerBase | Aceita |
| [CF-002](./CF-002-configurar-secoes-com-mapsection.md) | ConfigureInternal Deve Registrar Secoes com MapSection | Aceita |

### Handler Pipeline

| ADR | Titulo | Status |
|-----|--------|--------|
| [CF-003](./CF-003-handler-herda-configurationhandlerbase.md) | Handler Deve Herdar ConfigurationHandlerBase com LoadStrategy | Aceita |
| [CF-004](./CF-004-posicao-explicita-unica-no-pipeline.md) | Handlers Devem Ter Posicao Explicita e Unica no Pipeline | Aceita |
| [CF-005](./CF-005-escopo-handler-tres-niveis.md) | Escopo de Handler Define Quais Chaves Sao Processadas | Aceita |

### Acesso Tipado

| ADR | Titulo | Status |
|-----|--------|--------|
| [CF-006](./CF-006-acesso-tipado-expression-trees.md) | Acesso a Configuracao Deve Ser Tipado via Expression Trees | Aceita |

## Escopo

- **CF-***: Regras que definem como gerenciar configuracoes de forma tipada, extensivel e segura atraves de pipelines de handlers com escopo e caching.

## Navegacao

- [Voltar para ADRs](../README.md)
- [Infrastructure ADRs](../infrastructure/README.md)
- [Code Style ADRs](../code-style/README.md)
