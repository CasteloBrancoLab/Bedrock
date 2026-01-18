# Building Blocks Documentation Guide

Este documento orienta a criação de documentações para os Building Blocks do Bedrock, garantindo consistência e qualidade.

---

## 📚 Índice de Building Blocks

### Core

| Building Block | Descrição | Documentação |
|----------------|-----------|--------------|
| **Id** | Gerador de IDs monotônicos UUIDv7 | [id.md](core/ids/id.md) |
| **RegistryVersion** | Controle de versão monotônica para entidades | [registry-version.md](core/registry-versions/registry-version.md) |
| **CustomTimeProvider** | TimeProvider customizado para testes | [custom-time-provider.md](core/time-providers/custom-time-provider.md) |
| **TenantInfo** | Identificador imutável de tenant para multi-tenancy | [tenant-info.md](core/tenant-infos/tenant-info.md) |
| **BirthDate** | Cálculo preciso de idade com suporte a TimeProvider | [birth-date.md](core/birth-dates/birth-date.md) |
| **ExecutionContext** | Contexto de execução para observabilidade e auditoria | [execution-context.md](core/execution-contexts/execution-context.md) |
| **ValidationUtils** | Utilitários de validação com códigos padronizados | [validation-utils.md](core/validations/validation-utils.md) |

### Domain Entities

| Building Block | Descrição | Documentação |
|----------------|-----------|--------------|
| **EntityBase** | Classe base para entidades de domínio com suporte a Clone-Modify-Return | [entity-base.md](domain-entities/entity-base.md) |
| **EntityInfo** | Metadados consolidados de entidade (Id, Tenant, Auditoria, Versão) | [entity-info.md](domain-entities/models/entity-info.md) |
| **EntityChangeInfo** | Rastreamento de auditoria de criação e modificação | [entity-change-info.md](domain-entities/models/entity-change-info.md) |

---

## Estrutura de Documentação

Toda documentação de Building Block deve seguir esta estrutura:

### 1. Cabeçalho e Visão Geral

```markdown
# [Emoji] Nome do Building Block

Descrição concisa de 1-2 frases explicando o que o building block faz.

> [Emoji] **Visão Geral:** Resumo dos principais benefícios com métricas de performance quando aplicável.
```

**Exemplo:**
```markdown
# 📦 Id - Gerador de IDs Monotônicos UUIDv7

A classe `Id` fornece geração ultrarrápida de identificadores únicos baseados em UUIDv7, com ordenação temporal e garantia de monotonicidade por thread.

> 💡 **Visão Geral:** Gere IDs únicos e ordenáveis em ~70-75 nanosegundos, com **garantia de monotonicidade** e unicidade global sem locks.
```

### 2. Tabela Comparativa (Quando Aplicável)

Se o building block é uma alternativa a outras soluções, inclua uma tabela comparativa:

```markdown
## 🎯 Por Que Usar [Building Block] ao Invés de [Alternativas]?

| Característica | Alternativa 1 | **Building Block** | Alternativa 2 |
|----------------|---------------|-------------------|---------------|
| **Característica 1** | ❌/⚠️ Valor | ✅ **Valor** | ❌/⚠️ Valor |
| **Característica 2** | ❌/⚠️ Valor | ✅ **Valor** | ❌/⚠️ Valor |
```

Use os emojis para indicar:
- ✅ Bom/Suportado
- ⚠️ Parcial/Com ressalvas
- ❌ Ruim/Não suportado

### 3. Sumário

```markdown
## 📋 Sumário

- [Por Que Usar...](#-por-que-usar-)
- [Contexto: Por Que Existe](#-contexto-por-que-existe)
- [Problemas Resolvidos](#-problemas-resolvidos)
- [Funcionalidades](#-funcionalidades)
- [Como Usar](#-como-usar)
- [Impacto na Performance](#-impacto-na-performance)
- [Trade-offs](#-tradeoffs)
- [Exemplos Avançados](#-exemplos-avançados)
- [Referências](#-referências)
```

### 4. Contexto: Por Que Existe

Explique o problema real que motivou a criação do building block:

```markdown
## 🎯 Contexto: Por Que Existe

### O Problema Real

Descrição do problema em 1-2 parágrafos.

**Exemplo de desafios comuns:**

[Blocos de código mostrando abordagens problemáticas com ❌ e explicações]

### A Solução

[Bloco de código mostrando a abordagem correta com ✅ e benefícios]
```

**Padrão para blocos de código problemáticos:**
```csharp
❌ Abordagem problemática:
public class Example
{
    // código problemático com comentários ⚠️
}

❌ Problemas:
- Problema 1
- Problema 2
- Problema 3
```

**Padrão para blocos de código corretos:**
```csharp
✅ Abordagem com [Building Block]:
public class Example
{
    // código correto com comentários ✨
}

✅ Benefícios:
- Benefício 1
- Benefício 2
- Benefício 3
```

### 5. Problemas Resolvidos

Liste cada problema resolvido como uma seção numerada:

```markdown
## 🔧 Problemas Resolvidos

### 1. [Emoji] Título do Problema

**Problema:** Descrição breve.

#### 📚 Analogia: [Nome da Analogia]

[Analogia do mundo real explicando o problema e solução]

#### 💻 Impacto Real no Código

[Código mostrando ❌ antes e ✅ depois]

---

### 2. [Emoji] Próximo Problema

...
```

### 6. Funcionalidades

Liste as funcionalidades com exemplos de código:

```markdown
## ✨ Funcionalidades

### [Emoji] Nome da Funcionalidade

Descrição breve.

[Bloco de código com exemplo]

**Por quê é [característica]?**
- Razão 1
- Razão 2
```

### 7. Como Usar

Organize em cenários numerados do mais simples ao mais complexo:

```markdown
## 📖 Como Usar

### 1️⃣ Uso Básico - [Descrição]

[Bloco de código]

**Quando usar:** [Descrição do cenário]

---

### 2️⃣ Uso [Intermediário] - [Descrição]

[Bloco de código]

**Quando usar:** [Descrição do cenário]

---

### 3️⃣ Uso [Avançado] - [Descrição]

[Bloco de código]

**Quando usar:** [Descrição do cenário]
```

### 8. Impacto na Performance

Inclua benchmarks reais e análises detalhadas:

```markdown
## 📊 Impacto na Performance

### 💭 As Grandes Perguntas

#### **Pergunta 1: [Pergunta comum]?**

> "Citação da pergunta"

**Resposta:** [Resposta direta]

### 📈 Resultados do Benchmark

Ambiente de teste:
- **Hardware:** [Especificações]
- **SO:** [Sistema Operacional]
- **.NET:** [Versão]
- **Modo:** Release com otimizações

#### 📊 Tabela de Resultados

| Método | Mean | Error | Ratio | Allocated |
|--------|------|-------|-------|-----------|
| **Baseline** | X ns | Y ns | 1.00 | - |
| **Building Block** | X ns | Y ns | Z | - |
```

Use ASCII art para visualizações:

```
╔═══════════════════════════════════════════════════════════════════╗
║           TÍTULO DA VISUALIZAÇÃO                                  ║
╠═══════════════════════════════════════════════════════════════════╣
║                                                                   ║
║ Dados e análises aqui                                            ║
║                                                                   ║
╚═══════════════════════════════════════════════════════════════════╝
```

### 9. Limitações Críticas (Quando Aplicável)

```markdown
## ⚠️ LIMITAÇÃO CRÍTICA: [Nome da Limitação]

### 🚨 Problema: [Descrição]

**Severidade:** [Alta/Média/Baixa] para [contexto]

**Descrição do Problema:**
[Explicação detalhada]

### 📖 Cenário de Exemplo

[Código demonstrando o problema]

### 💥 Impacto por Padrão de Uso

| Padrão de Uso | Impacto | Análise |
|---------------|---------|---------|
| **Padrão 1** | **SEM IMPACTO** | Explicação |
| **Padrão 2** | **QUEBRA** | Explicação |

### 🛡️ Estratégias de Mitigação

#### 1️⃣ **[Estratégia 1]**

[Código e explicação]

#### 2️⃣ **[Estratégia 2]**

[Código e explicação]
```

### 10. Trade-offs

```markdown
## ⚖️ Trade-offs

### Benefícios

| Benefício | Impacto | Análise |
|-----------|---------|---------|
| **Benefício 1** | ✅ [Impacto] | [Explicação] |

### Custos

| Custo | Impacto | Mitigação |
|-------|---------|-----------|
| **Custo 1** | ⚠️ [Impacto] | [Como mitigar] |

### Quando Usar vs Quando Evitar

#### ✅ Use quando:
1. Cenário 1
2. Cenário 2

#### ❌ Evite quando:
1. Cenário 1
2. Cenário 2
```

### 11. Exemplos Avançados

```markdown
## 🔬 Exemplos Avançados

### [Emoji] [Nome do Exemplo]

[Descrição do cenário avançado]

[Bloco de código completo e funcional]

**Pontos importantes:**
- Ponto 1
- Ponto 2
```

### 12. Referências

```markdown
## 📚 Referências

- [Nome do recurso](URL) - Descrição breve
- [RFC/Especificação](URL) - Descrição
```

---

## Convenções de Estilo

### Emojis por Seção

| Seção | Emoji |
|-------|-------|
| Cabeçalho principal | 📦 ⏰ 🔑 🛡️ |
| Visão geral | 💡 |
| Por que usar | 🎯 |
| Sumário | 📋 |
| Contexto | 🎯 |
| Problemas resolvidos | 🔧 🔴 |
| Funcionalidades | ✨ 💚 |
| Como usar | 📖 🚀 |
| Performance | 📊 📈 |
| Limitações | ⚠️ 🚨 |
| Trade-offs | ⚖️ |
| Exemplos avançados | 🔬 |
| Referências | 📚 |

### Emojis para Status

- ✅ Correto/Bom/Recomendado
- ❌ Incorreto/Ruim/Evitar
- ⚠️ Atenção/Parcial/Cuidado
- ✨ Destaque positivo
- 🚀 Alta performance
- ⭐ Recomendado

### Formatação de Código

1. **Comentários inline:** Use emojis para chamar atenção
   ```csharp
   var id = Id.GenerateNewId();  // ✨ Rápido e ordenável
   var bad = Guid.NewGuid();     // ⚠️ Aleatório, não ordenável
   ```

2. **Blocos de problema/solução:** Sempre indique claramente
   ```csharp
   ❌ Código problemático:
   // código ruim

   ✅ Código correto:
   // código bom
   ```

3. **Resultados de benchmark:** Use formato de tabela Markdown

### Linguagem

- Documentação em **português brasileiro**
- Termos técnicos podem permanecer em inglês (thread-safe, benchmark, etc.)
- Use voz ativa e direta
- Evite jargões desnecessários
- Explique conceitos complexos com analogias do mundo real

### Métricas de Performance

Sempre inclua:
- Ambiente de teste completo (hardware, SO, versão .NET)
- Números absolutos (nanosegundos, bytes)
- Comparações relativas (% mais rápido, Nx melhor)
- Impacto prático em cenários reais

---

## Checklist de Documentação

Antes de finalizar, verifique:

- [ ] Cabeçalho com emoji, nome e descrição concisa
- [ ] Tabela comparativa (se aplicável)
- [ ] Sumário com links funcionais
- [ ] Contexto explicando o problema real
- [ ] Pelo menos 2-3 problemas resolvidos com analogias
- [ ] Lista de funcionalidades com exemplos
- [ ] Guia de uso do básico ao avançado
- [ ] Benchmarks com ambiente e metodologia
- [ ] Limitações críticas documentadas (se houver)
- [ ] Trade-offs claros
- [ ] Exemplos avançados funcionais
- [ ] Referências externas

---

## Arquivos de Referência

Para exemplos completos de documentação, consulte o [Índice de Building Blocks](#-índice-de-building-blocks) no início deste documento.
