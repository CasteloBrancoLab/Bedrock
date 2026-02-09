# Research: Auth Domain Model

**Feature**: 001-auth-domain-model
**Date**: 2026-02-09

## R1: Biblioteca Argon2id para .NET

**Decision**: Usar `Konscious.Security.Cryptography` (NuGet package)

**Rationale**: Biblioteca .NET pura, amplamente utilizada, suporta Argon2id nativamente. Não requer dependências nativas (libsodium). Compatível com .NET 10.0. Permite configuração granular de parâmetros (memory, iterations, parallelism).

**Alternatives considered**:
- `Isopoh.Cryptography.Argon2` — Menor adoção, API menos intuitiva
- `libsodium-net` — Requer binding nativo (complexidade de deploy), mas tem implementação mais madura. Rejeitado por simplicidade e portabilidade cross-platform
- `NSec.Cryptography` — Foco em curvas elípticas, Argon2 não é foco principal
- Implementação nativa .NET 10.0 — `System.Security.Cryptography` não inclui Argon2id nativamente ainda

## R2: Parâmetros Argon2id (OWASP)

**Decision**: Usar parâmetros OWASP recomendados como defaults configuráveis

**Rationale**: OWASP recomenda para Argon2id:
- Memory: 19 MiB (19456 KiB)
- Iterations: 2
- Parallelism: 1
- Salt length: 16 bytes (gerado automaticamente)
- Hash length: 32 bytes

Estes valores equilibram segurança e performance. São configuráveis no PasswordPolicyMetadata para ajuste por ambiente (development pode usar valores menores para testes rápidos).

**Alternatives considered**:
- NIST SP 800-63B — Não especifica parâmetros concretos para Argon2id, apenas recomenda o algoritmo
- RFC 9106 — Define o algoritmo mas não recomenda parâmetros específicos para produção

## R3: Estratégia de Pepper

**Decision**: Pepper aplicado via HMAC-SHA256 antes do Argon2id, com versionamento

**Rationale**: O pepper é uma chave secreta da aplicação que adiciona uma camada de defesa em profundidade. Aplicar via HMAC-SHA256(pepper, password) antes do Argon2id é a abordagem recomendada porque:
1. HMAC é determinístico — mesmo pepper + mesma senha = mesmo resultado intermediário
2. Argon2id adiciona salt aleatório sobre o resultado do HMAC — garante unicidade do hash final
3. Versionamento permite rotação gradual sem invalidar hashes existentes
4. O hash armazenado inclui a versão do pepper usada (como prefixo ou campo separado)

**Alternatives considered**:
- Pepper concatenado à senha — Menos seguro que HMAC, vulnerável a ataques de extensão de comprimento
- Pepper como parâmetro adicional do Argon2id — Argon2id não tem campo nativo para pepper
- Sem pepper — Menos seguro, hash comprometido no banco pode ser brute-forced offline

## R4: Formato de Armazenamento do Hash

**Decision**: Byte array opaco contendo: versão do pepper (1 byte) + salt (16 bytes) + hash (32 bytes) = 49 bytes total

**Rationale**: Formato binário compacto sem overhead de encoding. A entidade User armazena como `byte[]` opaco — apenas o building block Security sabe interpretar a estrutura interna. Isso mantém o desacoplamento total entre Domain.Entities e Security.

**Alternatives considered**:
- PHC string format ($argon2id$v=19$m=...) — Texto legível mas maior e requer parsing. A versão do pepper não é padrão no formato PHC
- Campos separados (salt, hash, pepper_version) — Expõe estrutura interna ao domínio, quebrando encapsulamento
- Base64 encoded string — Overhead de encoding/decoding desnecessário quando byte[] é suficiente

## R5: Value Object PasswordHash vs byte[] direto

**Decision**: Criar value object `PasswordHash` como readonly struct wrapping byte[]

**Rationale**: Seguindo o padrão Bedrock de value objects (como EmailAddress, Id, BirthDate), encapsular o byte[] em um readonly struct fornece:
1. Type safety — não confundir com outros byte[] no sistema
2. Comparação segura em tempo constante (CryptographicOperations.FixedTimeEquals)
3. Validação de tamanho no construtor
4. ToString() que nunca expõe o conteúdo (retorna "[REDACTED]")
5. Zero-allocation como struct

**Alternatives considered**:
- byte[] direto — Sem type safety, sem comparação segura built-in, ToString() exporia conteúdo
- ReadOnlyMemory<byte> — Mais complexo, não necessário para 49 bytes fixos

## R6: User Entity — Arquétipo SimpleAggregateRoot

**Decision**: User segue o arquétipo SimpleAggregateRoot (sealed, sem herança, sem coleções filhas)

**Rationale**: A entidade User nesta fase não tem entidades filhas nem hierarquia de herança. É o caso mais simples do Bedrock. O template SimpleAggregateRoot fornece todos os padrões necessários: factory methods, Clone-Modify-Return, metadata estático, Input objects, validação com &.

**Alternatives considered**:
- CompositeAggregateRoot — User não gerencia coleções filhas nesta fase
- AbstractAggregateRoot — Não há necessidade de herança (admin user, regular user são diferenciados por status/roles, não por tipo)

## R7: Transições de Estado UserStatus

**Decision**: Máquina de estados com validação explícita de transições permitidas

**Rationale**: As transições permitidas são:
```
Ativo → Suspenso ✓
Ativo → Bloqueado ✓
Suspenso → Ativo ✓
Suspenso → Bloqueado ✓
Bloqueado → Ativo ✓
Bloqueado → Suspenso ✗ (bloqueio requer reativação completa)
```

Implementação via método estático `IsValidTransition(UserStatus from, UserStatus to)` que retorna bool. Transições inválidas adicionam mensagem ao ExecutionContext.

**Alternatives considered**:
- State pattern (classe por estado) — Over-engineering para 3 estados simples
- Sem validação (aceitar qualquer transição) — Permite estados inconsistentes
