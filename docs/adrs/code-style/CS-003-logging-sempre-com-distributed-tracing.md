# CS-003: Logging Sempre com Distributed Tracing

## Status

Aceita

## Contexto

### O Problema (Analogia)

Imagine um hospital com milhares de pacientes. Cada exame de sangue
gera um resultado num papel. Se o papel tem o nome do paciente, o
numero do prontuario e a data da coleta, qualquer medico encontra o
resultado no instante em que precisa. Se o papel tem apenas "Glicose:
120 mg/dL" sem identificacao, o resultado vira lixo — impossivel saber
de quem é, quando foi coletado, ou a qual consulta pertence.

Agora imagine que o hospital tem 50 laboratorios em cidades
diferentes. Um resultado sem identificacao num laboratorio remoto é
pior que inutil — é um desperdicio de recurso. Um resultado com
prontuario, data e origem do pedido pode ser correlacionado com
qualquer evento em qualquer outro laboratorio.

### O Problema Tecnico

A API padrao do `ILogger` (`Log`, `LogError`, `LogWarning`,
`LogInformation`, etc.) registra mensagens sem nenhum contexto de rastreamento
distribuido. Num sistema multi-tenant com multiplos servicos, um log
sem `CorrelationId`, `TenantCode`, `ExecutionUser` e
`BusinessOperationCode` é praticamente inutil em producao.

O Bedrock fornece extension methods em
`Bedrock.BuildingBlocks.Observability.ExtensionMethods` que adicionam
automaticamente 7 campos de contexto a cada log entry via
`ILogger.BeginScope`. Usar a API padrao do `ILogger` em vez dessas
extension methods é uma violacao que gera logs orfaos — presentes no
sistema mas impossíveis de correlacionar com a operacao de negocio que
os gerou.

## Como Normalmente É Feito

### Abordagem Tradicional

A maioria dos projetos usa `ILogger` diretamente:

```csharp
public class UserRepository : RepositoryBase<User>
{
    public async Task<User?> GetByEmailAsync(
        ExecutionContext executionContext,
        EmailAddress email,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _postgreSqlRepository.GetByEmailAsync(
                executionContext, email, cancellationToken);
        }
        catch (Exception ex)
        {
            // ❌ Log sem contexto de rastreamento
            Logger.LogError(ex, "An error occurred while getting user by email.");
            return null;
        }
    }
}
```

O log resultante contem apenas:

```json
{
  "Level": "Error",
  "Message": "An error occurred while getting user by email.",
  "Exception": "Npgsql.PostgresException: ..."
}
```

### Por Que Nao Funciona Bem

- **Sem correlacao**: Se o erro aconteceu durante uma chamada HTTP que
  passou por 3 servicos, nao há como ligar este log aos logs dos
  outros servicos. O `CorrelationId` nao foi registrado.
- **Sem tenant**: Em sistema multi-tenant, nao há como filtrar logs
  por tenant. O SRE precisa adivinhar qual cliente foi afetado.
- **Sem usuario**: Nao há como saber qual usuario disparou a operacao
  que falhou. Investigacao de incidentes fica manual.
- **Sem operacao de negocio**: Um erro "getting user by email" pode
  ter sido disparado por login, por reset de senha, por validacao de
  cadastro. Sem `BusinessOperationCode`, o contexto é perdido.
- **Sem timestamp do contexto**: O timestamp do log pode diferir do
  timestamp da operacao de negocio (atraso de flush, buffering).

O problema é agravado pelo fato de que o `ExecutionContext` ja está
disponivel no escopo do metodo — é um parametro. Nao usar é
desperdicar informacao que ja existe.

### A Armadilha do "Funciona Local"

Em ambiente de desenvolvimento, `Logger.LogError` funciona
perfeitamente. O desenvolvedor ve o erro no console, sabe exatamente
o que estava fazendo, e resolve. A violacao so aparece em producao,
quando o log é um entre milhoes e ninguem sabe quem, quando, ou por
que.

## A Decisao

### Nossa Abordagem

Todo log emitido em codigo que possui `ExecutionContext` disponivel
DEVE usar os extension methods de distributed tracing em vez da API
padrao do `ILogger`.

**Correto** — log com distributed tracing (mesmo codigo do
`RepositoryBase`):

```csharp
catch (Exception ex)
{
    Logger.LogExceptionForDistributedTracing(
        executionContext,
        ex,
        "An error occurred while getting user by email.");
    return null;
}
```

**Incorreto** — log padrao do ILogger:

```csharp
catch (Exception ex)
{
    Logger.LogError(ex, "An error occurred while getting user by email.");
    return null;
}
```

### Metodos Disponiveis

A API de logging com distributed tracing oferece 7 familias de
metodos, cada uma com overloads genericos para 0-3 argumentos (zero
alocacao) e um fallback `params object[]`:

| Familia | Nivel | Uso |
|---------|-------|-----|
| `LogTraceForDistributedTracing` | Trace | Detalhes internos de diagnostico |
| `LogDebugForDistributedTracing` | Debug | Informacoes uteis para depuracao |
| `LogInformationForDistributedTracing` | Information | Fluxo normal da aplicacao |
| `LogWarningForDistributedTracing` | Warning | Situacoes anormais mas recuperaveis |
| `LogErrorForDistributedTracing` | Error | Erros sem excecao associada |
| `LogCriticalForDistributedTracing` | Critical | Falhas graves que requerem atencao imediata |
| `LogExceptionForDistributedTracing` | Error | Erros com excecao associada (nivel Error automatico) |

Adicionalmente, o metodo generico `LogForDistributedTracing` aceita
`LogLevel` como parametro para casos dinamicos.

### Dados Incluidos Automaticamente (7 campos)

Cada chamada cria um `ILogger.BeginScope` com `ExecutionContextScope`
(struct, zero alocacao no heap) contendo:

| Campo | Origem | Exemplo |
|-------|--------|---------|
| `Timestamp` | `ExecutionContext.Timestamp` | `2025-01-15T10:30:00Z` |
| `CorrelationId` | `ExecutionContext.CorrelationId` | `3fa85f64-5717-4562-b3fc-2c963f66afa6` |
| `TenantCode` | `ExecutionContext.TenantInfo.Code` | `ACME` |
| `TenantName` | `ExecutionContext.TenantInfo.Name` | `Acme Corporation` |
| `ExecutionUser` | `ExecutionContext.ExecutionUser` | `john.doe@acme.com` |
| `ExecutionOrigin` | `ExecutionContext.ExecutionOrigin` | `WebAPI` |
| `BusinessOperationCode` | `ExecutionContext.BusinessOperationCode` | `AUTH-LOGIN` |

### Escopo da Regra

| Aspecto | Definicao |
|---------|-----------|
| Obrigatorio | Qualquer metodo que recebe ou tem acesso a `ExecutionContext` |
| Isento | Codigo de infraestrutura de bootstrap (Program.cs, Startup) onde `ExecutionContext` nao existe |
| Isento | Playground e apps de console para demonstracao rapida |
| Isento | Testes unitarios (logging é mock) |
| Violacao | Usar `Log`, `LogError`, `LogWarning`, `LogInformation`, etc. do `ILogger` quando `ExecutionContext` está disponivel |

### Por Que Funciona Melhor

1. **Correlacao ponta a ponta**: Todo log pode ser filtrado por
   `CorrelationId`, ligando eventos entre multiplos servicos.
2. **Isolamento por tenant**: Filtrar por `TenantCode` isola logs
   de um cliente sem ruido dos demais.
3. **Auditoria**: `ExecutionUser` e `BusinessOperationCode`
   permitem reconstruir a sequencia de acoes de um usuario.
4. **Zero custo adicional**: A API usa overloads genericos para
   0-3 argumentos, evitando a alocacao de `params object[]`.
   `ExecutionContextScope` é uma struct que implementa
   `IReadOnlyList<KeyValuePair<string, object?>>` sem heap
   allocation.
5. **Early exit otimizado**: O metodo core usa
   `[MethodImpl(MethodImplOptions.AggressiveInlining)]` e verifica
   `logger.IsEnabled(logLevel)` antes de qualquer trabalho —
   se o nivel esta desabilitado, o custo é zero.

## Consequencias

### Beneficios

- Logs orfaos (sem contexto) sao eliminados.
- Investigacao de incidentes em producao se torna possivel sem acesso
  ao ambiente ou reproducao do cenario.
- Dashboards de observabilidade (Grafana, Datadog, Application
  Insights) funcionam corretamente com filtros de correlacao e tenant.
- Code agents geram logging correto por padrao — a API é tao simples
  quanto a padrao do `ILogger`, com um parametro adicional.

### Trade-offs (Com Perspectiva)

- **Parametro adicional**: Cada chamada de log exige
  `executionContext` como primeiro argumento. Na pratica, o
  `ExecutionContext` ja é parametro de todos os metodos de repositorio,
  service e handler — é um argumento que ja existe no escopo.
- **Nome mais longo**: `LogExceptionForDistributedTracing` é mais
  verboso que `LogError`. A verbosidade é intencional: o nome explicita
  o que o metodo faz, e o IntelliSense resolve a digitacao. Um nome
  curto que esconde o que faz é pior que um nome longo que revela.
- **Dependencia do Observability**: Projetos que usam a API precisam
  referenciar `Bedrock.BuildingBlocks.Observability`. Esse building
  block ja é referenciado por todos os projetos de infraestrutura,
  entao o custo é zero na pratica.

## Fundamentacao Teorica

### Padroes de Design Relacionados

**Correlation Pattern** (Distributed Systems): Todo evento em um
sistema distribuido deve carregar um identificador de correlacao que
permite rastrear a cadeia completa de chamadas. O `ExecutionContext`
do Bedrock materializa esse padrao com `CorrelationId`,
`ExecutionUser` e `BusinessOperationCode`.

**Scope Pattern** (Microsoft.Extensions.Logging): O `ILogger.BeginScope`
cria um escopo onde todos os logs emitidos herdam propriedades
adicionais. O `ExecutionContextScope` usa esse mecanismo nativo para
injetar os 7 campos de contexto sem modificar o provider de logging.

### O Que o Observability Engineering Diz

> "Logs without context are noise. Logs with context are signals."
>
> *Logs sem contexto sao ruido. Logs com contexto sao sinais.*

Charity Majors, *Observability Engineering* (2022). O principio
central da observabilidade moderna é que cada evento deve ser
auto-descritivo: quem, quando, onde, por que. Um log sem esses dados
exige que o investigador reconstrua o contexto manualmente — trabalho
que a maquina deveria ter feito no momento da emissao.

### O Que o .NET Logging Guidelines Diz

> "Use scopes to add context to log messages. Scoped values are
> automatically included in all log entries within the scope."
>
> *Use escopos para adicionar contexto a mensagens de log. Valores
> de escopo sao automaticamente incluidos em todas as entradas de
> log dentro do escopo.*

A documentacao oficial do .NET recomenda o uso de `BeginScope` para
enriquecer logs com dados contextuais. O `LogForDistributedTracing`
encapsula esse padrao, evitando que cada desenvolvedor tenha que
lembrar de abrir e fechar escopos manualmente.

### Outros Fundamentos

**Structured Logging**: Os 7 campos do `ExecutionContextScope` sao
key-value pairs tipados, nao strings concatenadas. Isso permite
filtragem eficiente em ferramentas como Seq, Elastic, Datadog e
Application Insights — em vez de `grep` em texto livre.

**Zero-Allocation Logging**: Os overloads genericos para 0-3
argumentos evitam a criacao do array `params object[]` que a API
padrao do `ILogger` exige. Em hot paths (repositorios, handlers),
essa diferenca é mensuravel. A comparacao com o `ILogger` padrao:

| Aspecto | `ILogger.LogError` | `LogExceptionForDistributedTracing` |
|---------|-------------------|-------------------------------------|
| Alocacao de args | `params object[]` (heap) | Overloads genericos (zero alloc para 0-3 args) |
| Contexto de escopo | Nenhum | 7 campos via struct |
| Early exit | Nao (args ja alocados) | Sim (`IsEnabled` antes de qualquer trabalho) |

## Aprenda Mais

### Perguntas Para Fazer a LLM

1. "Qual a diferenca entre structured logging e text logging em
   termos de performance de busca?"
2. "Como o `ILogger.BeginScope` funciona internamente e qual o custo
   de criar um escopo?"
3. "Por que correlation IDs sao essenciais em arquiteturas de
   microsservicos?"
4. "Como o modificador `AggressiveInlining` ajuda no early-exit de
   logging desabilitado?"

### Leitura Recomendada

- Charity Majors, Liz Fong-Jones, George Miranda,
  *Observability Engineering* (2022)
- [.NET Logging Guidance — Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging)
- [High-performance logging in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/high-performance-logging)
- [Distributed Tracing — OpenTelemetry](https://opentelemetry.io/docs/concepts/signals/traces/)

## Building Blocks Correlacionados

| Building Block | Relacao com a ADR |
|----------------|-------------------|
| Observability | Define os extension methods `Log*ForDistributedTracing` e o `ExecutionContextScope` |
| Core | Define `ExecutionContext`, `TenantInfo`, `CorrelationId` e demais dados de contexto |
| Data | `RepositoryBase` é a implementacao de referencia que usa `LogExceptionForDistributedTracing` corretamente |
| Persistence.PostgreSql | `DataModelRepositoryBase` e `PostgreSqlUnitOfWorkBase` usam `LogExceptionForDistributedTracing` |

## Referencias no Codigo

- API de logging: `src/BuildingBlocks/Observability/ExtensionMethods/LoggerExtensionMethods.cs`
- Scope struct: `ExecutionContextScope` no mesmo arquivo
- Uso correto (RepositoryBase): `src/BuildingBlocks/Data/Repositories/RepositoryBase.cs` (linhas 129, 150, 171, 192, 222)
- Uso correto (DataModelRepositoryBase): `src/BuildingBlocks/Persistence.PostgreSql/DataModelRepositories/DataModelRepositoryBase.cs`
- Uso correto (UnitOfWork): `src/BuildingBlocks/Persistence.PostgreSql/UnitOfWork/PostgreSqlUnitOfWorkBase.cs`
- Uso correto (Auth UserRepository): `src/ShopDemo/Auth/Infra.Data/Repositories/UserRepository.cs` (linhas 44, 67, 90, 113 — usa `Logger.LogExceptionForDistributedTracing`)
