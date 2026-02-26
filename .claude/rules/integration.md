---
paths:
  - "tests/IntegrationTests/**"
  - "src/BuildingBlocks/Testing/Integration/**"
---

# Convencoes de Testes de Integracao

## Testcontainers + Docker

Os testes de integracao usam **Testcontainers** que requer Docker disponivel.

### Docker no WSL2 (Windows)

```bash
export DOCKER_HOST=tcp://127.0.0.1:2375
```

> **IMPORTANTE**: Usar `127.0.0.1` e **nao** `localhost`. O .NET resolve `localhost` para IPv6 (`::1`) primeiro, causando timeouts de ~21s.

### Auto-configuracao (DockerHostSetup)

O `[ModuleInitializer]` em `src/BuildingBlocks/Testing/Integration/DockerHostSetup.cs` corrige automaticamente:

1. **IPv6 → IPv4**: Substitui `localhost` por `127.0.0.1` no `DOCKER_HOST`
2. **Ryuk socket**: Configura `TESTCONTAINERS_DOCKER_SOCKET_OVERRIDE=/var/run/docker.sock`

Basta ter `DOCKER_HOST` definido — o resto e automatico.

## Nomenclatura

- Formato: `Bedrock.IntegrationTests.<namespace-do-src>`
- Relacao **1:1** com projeto src (igual a unit tests)
