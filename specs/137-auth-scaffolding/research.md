# Research: Auth - Estrutura dos Projetos (Scaffolding)

**Date**: 2026-02-08
**Feature**: [spec.md](spec.md)

## Status

Scaffolding feature — pesquisa mínima necessária. Todas as decisões técnicas já estão definidas pela issue #136 e pelas convenções existentes do ShopDemo.

## Decisões

### D1: Convenção de Nomenclatura

**Decisão**: Usar `ShopDemo.Auth.*` seguindo a convenção do ShopDemo.
**Razão**: Os módulos existentes (Customers, Orders, Products) usam `ShopDemo.{Module}.*`. Manter consistência.
**Alternativas**: `Bedrock.Auth.*` (rejeitada — não segue convenção ShopDemo).

### D2: Localização dos Projetos

**Decisão**: `src/ShopDemo/Auth/` (correção do usuário).
**Razão**: O Auth é um módulo do ShopDemo sample, não um BuildingBlock do Bedrock. A issue #137 originalmente propunha `src/Services/Auth/` mas o usuário corrigiu para manter dentro do ShopDemo.
**Alternativas**: `src/Services/Auth/` (rejeitada — Auth não é um serviço do framework, é um sample).

### D3: Camadas do Auth

**Decisão**: 5 camadas (Domain.Entities, Application, Infra.Data, Infra.Data.PostgreSql, Api).
**Razão**: Definido pela arquitetura na issue #136. Cada camada tem responsabilidade clara na arquitetura limpa.
**Alternativas**: Nenhuma considerada — arquitetura definida previamente.

### D4: Registro no Teste de Arquitetura

**Decisão**: Adicionar path do `ShopDemo.Auth.Domain.Entities` ao `DomainEntitiesArchFixture`.
**Razão**: As 58 regras arquiteturais (DE001-DE058) DEVEM validar as entidades do Auth desde o início. Identificado durante `/speckit.clarify`.
**Alternativas**: Não registrar (rejeitada — entidades ficariam sem validação arquitetural).

## NEEDS CLARIFICATION

Nenhum — todos os aspectos foram resolvidos pelo spec.md, clarify e convenções existentes.
