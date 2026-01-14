using System.Collections.Concurrent;
using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models;
using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.TenantInfos;

namespace Bedrock.BuildingBlocks.Core.ExecutionContexts;

// ExecutionContext é uma classe core que atua como observador passivo durante a execução de operações.
// Ela coleta mensagens de diferentes níveis de severidade e exceções para fins de auditoria,
// diagnóstico e observabilidade.
//
// DECISÕES DE DESIGN [NÃO ALTERAR SEM JUSTIFICATIVA FORTE]:
//
// 1. VALIDAÇÕES LIMITADAS AO ESSENCIAL [INTENCIONAL]
//    - Valida apenas tipos referência não-nulos (timeProvider, executionUser, exception)
//    - NÃO valida regras de negócio (Guid.Empty, valores default de TenantInfo)
//    - Motivo: Camada de negócio garante valores válidos antes de criar o contexto
//    - NÃO adicionar validações de Guid.Empty ou TenantInfo - viola Single Responsibility Principle
//
// 2. PROPRIEDADES CALCULADAS EM GETTERS (não cache) [INTENCIONAL]
//    - HasErrorMessages, IsPartiallySuccessful iteram a coleção a cada acesso
//    - Padrão de uso: write-heavy (muitas mensagens), read-light (poucas consultas no final)
//    - Motivo: Simplicidade e thread-safety > otimização prematura
//    - NÃO adicionar cache - complexidade desnecessária para o padrão de uso real
//
// 3. ChangeMessageType NÃO VALIDA MinimumMessageType [INTENCIONAL]
//    - Permite downgrade (Error → Warning) para importações parciais bem-sucedidas
//    - Permite upgrade (Warning → Error) para políticas mais restritivas
//    - Motivo: MinimumMessageType filtra na ENTRADA; ChangeMessageType ajusta SEMÂNTICA de negócio
//    - NÃO adicionar validação de MinimumMessageType - quebra casos de uso válidos
//
// 4. MÉTODOS Change* RETORNAM bool [INTENCIONAL]
//    - Retorna true se mensagem foi modificada, false se ID não existe
//    - Motivo: API de alto nível - desenvolvedores podem verificar sucesso da operação
//    - Falha silenciosa (void) seria confusa para usuários menos experientes
//
// 5. USO CORRETO - NÃO USAR PARA CONTROLE DE FLUXO [IMPORTANTE]
//    - Métodos devem retornar seu próprio status (bool, Result<T>, exceções)
//    - ExecutionContext observa, não controla
//    - Propriedades consultadas no FINAL para diagnóstico/auditoria
//    - NÃO usar context.HasErrorMessages para controlar fluxo durante execução
//
// EXEMPLO DE USO CORRETO:
// public Result ProcessOrder(Order order, ExecutionContext context)
// {
//     if (!ValidateOrder(order))
//     {
//         context.AddErrorMessage(tp, "INVALID_ORDER", "Validation failed");
//         return Result.Failure("Validation failed");  // ✅ Método retorna falha
//     }
//
//     context.AddSuccessMessage(tp, "ORDER_PROCESSED", $"Order {order.Id}");
//     return Result.Success(order);  // ✅ Método retorna sucesso
// }
//
// // No final, usa o contexto para diagnóstico
// if (!context.IsSuccessful)
//     _logger.LogWarning("Processing had issues: {Messages}", context.Messages);
public class ExecutionContext
{
    // Fields
    private readonly ConcurrentDictionary<Id, Message> _messageCollection;
    private readonly ConcurrentBag<Exception> _exceptionCollection;

    // Properties
    public DateTimeOffset Timestamp { get; }
    public Guid CorrelationId { get; }
    public TenantInfo TenantInfo { get; }
    public string ExecutionUser { get; }
    public string ExecutionOrigin { get; }
    public string BusinessOperationCode { get; private set; }
    public MessageType MinimumMessageType { get; }
    public TimeProvider TimeProvider { get; }

    public bool HasMessages
    {
        get
        {
            return !_messageCollection.IsEmpty;
        }
    }
    public bool HasErrorMessages
    {
        get
        {
            foreach (Message message in _messageCollection.Values)
            {
                if (message.MessageType is MessageType.Error or MessageType.Critical)
                {
                    return true;
                }
            }

            return false;
        }
    }
    public bool HasExceptions
    {
        get
        {
            return !_exceptionCollection.IsEmpty;
        }
    }

    public bool IsSuccessful
    {
        get
        {
            return !HasErrorMessages && !HasExceptions;
        }
    }
    public bool IsFaulted
    {
        get
        {
            return HasExceptions || HasErrorMessages;
        }
    }
    public bool IsPartiallySuccessful
    {
        get
        {
            bool hasSuccessMessages = false;
            foreach (Message message in _messageCollection.Values)
            {
                if (message.MessageType == MessageType.Success)
                {
                    hasSuccessMessages = true;
                    // Stryker disable once Statement : Remover break e equivalente funcionalmente - apenas otimizacao de performance
                    break;
                }
            }

            return hasSuccessMessages && IsFaulted;
        }
    }

    public IEnumerable<Message> Messages
    {
        get
        {
            return _messageCollection.Values;
        }
    }
    public IEnumerable<Exception> Exceptions
    {
        get
        {
            return _exceptionCollection;
        }
    }

    // Constructors
    private ExecutionContext(
        DateTimeOffset timestamp,
        Guid correlationId,
        TenantInfo tenantInfo,
        string executionUser,
        string executionOrigin,
        string businessOperationCode,
        MessageType minimumMessageType,
        TimeProvider timeProvider,
        ConcurrentDictionary<Id, Message> messageCollection,
        ConcurrentBag<Exception> exceptionCollection
    )
    {
        Timestamp = timestamp;
        CorrelationId = correlationId;
        TenantInfo = tenantInfo;
        ExecutionUser = executionUser;
        ExecutionOrigin = executionOrigin;
        BusinessOperationCode = businessOperationCode;
        MinimumMessageType = minimumMessageType;
        TimeProvider = timeProvider;

        _messageCollection = messageCollection;
        _exceptionCollection = exceptionCollection;
    }

    // Public Methods
    public static ExecutionContext Create(
        Guid correlationId,
        TenantInfo tenantInfo,
        string executionUser,
        string executionOrigin,
        string businessOperationCode,
        MessageType minimumMessageType,
        TimeProvider timeProvider
    )
    {
        ArgumentNullException.ThrowIfNull(timeProvider, nameof(timeProvider));
        ArgumentException.ThrowIfNullOrWhiteSpace(executionUser, nameof(executionUser));
        ArgumentException.ThrowIfNullOrWhiteSpace(executionOrigin, nameof(executionOrigin));
        ArgumentException.ThrowIfNullOrWhiteSpace(businessOperationCode, nameof(businessOperationCode));
        Message.ThrowIfInvalidMessageType(minimumMessageType);

        return new ExecutionContext(
            timestamp: timeProvider.GetUtcNow(),
            correlationId: correlationId,
            tenantInfo: tenantInfo,
            executionUser: executionUser,
            executionOrigin: executionOrigin,
            businessOperationCode: businessOperationCode,
            minimumMessageType: minimumMessageType,
            timeProvider: timeProvider,
            messageCollection: [],
            exceptionCollection: []
        );
    }
    public ExecutionContext Clone()
    {
        return new ExecutionContext(
            timestamp: Timestamp,
            correlationId: CorrelationId,
            tenantInfo: TenantInfo,
            executionUser: ExecutionUser,
            executionOrigin: ExecutionOrigin,
            businessOperationCode: BusinessOperationCode,
            minimumMessageType: MinimumMessageType,
            timeProvider: TimeProvider,
            messageCollection: new ConcurrentDictionary<Id, Message>(_messageCollection),
            exceptionCollection: [.. _exceptionCollection]
        );
    }

    public void Import(ExecutionContext other)
    {
        ArgumentNullException.ThrowIfNull(other);

        foreach (Message message in other._messageCollection.Values)
        {
            _ = _messageCollection.TryAdd(message.Id, message);
        }

        foreach (Exception exception in other._exceptionCollection)
        {
            _exceptionCollection.Add(exception);
        }
    }

    public void AddException(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        _exceptionCollection.Add(exception);
    }

    public void AddTraceMessage(
        string code,
        string? text = null
    )
    {
        if (!CanAddMessage(MessageType.Trace, MinimumMessageType))
            return;

        AddMessage(
            Message.CreateTrace(
                TimeProvider,
                code,
                text
            )
        );
    }

    public void AddDebugMessage(
        string code,
        string? text = null
    )
    {
        if (!CanAddMessage(MessageType.Debug, MinimumMessageType))
            return;

        AddMessage(
            Message.CreateDebug(
                TimeProvider,
                code,
                text
            )
        );
    }

    public void AddInformationMessage(
        string code,
        string? text = null
    )
    {
        if (!CanAddMessage(MessageType.Information, MinimumMessageType))
            return;

        AddMessage(
            Message.CreateInformation(
                TimeProvider,
                code,
                text
            )
        );
    }

    public void AddWarningMessage(
        string code,
        string? text = null
    )
    {
        if (!CanAddMessage(MessageType.Warning, MinimumMessageType))
            return;

        AddMessage(
            Message.CreateWarning(
                TimeProvider,
                code,
                text
            )
        );
    }

    public void AddErrorMessage(
        string code,
        string? text = null
    )
    {
        AddMessage(
            Message.CreateError(
                TimeProvider,
                code,
                text
            )
        );
    }

    public void AddCriticalMessage(
        string code,
        string? text = null
    )
    {
        AddMessage(
            Message.CreateCritical(
                TimeProvider,
                code,
                text
            )
        );
    }

    public void AddSuccessMessage(
        string code,
        string? text = null
    )
    {
        // Success messages are always added, regardless of MinimumMessageType

        AddMessage(
            Message.CreateSuccess(
                TimeProvider,
                code,
                text
            )
        );
    }

    public bool ChangeMessageText(Id messageId, string? newText)
    {
        if (!_messageCollection.TryGetValue(messageId, out Message message))
            return false;

        _messageCollection[messageId] = message.WithText(newText);
        return true;
    }

    public bool ChangeMessageType(Id messageId, MessageType newMessageType)
    {
        if (!_messageCollection.TryGetValue(messageId, out Message message))
            return false;

        _messageCollection[messageId] = message.WithMessageType(newMessageType);
        return true;
    }

    public bool ChangeMessageType(MessageType oldMessageType, MessageType newMessageType)
    {
        bool anyChanged = false;

        foreach (Message message in _messageCollection.Values)
        {
            if (message.MessageType == oldMessageType)
            {
                _messageCollection[message.Id] = message.WithMessageType(newMessageType);
                anyChanged = true;
            }
        }

        return anyChanged;
    }

    public void ChangeBusinessOperationCode(string newBusinessOperationCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newBusinessOperationCode, nameof(newBusinessOperationCode));

        BusinessOperationCode = newBusinessOperationCode;
    }

    public Dictionary<string, object?> ToDictionary()
    {
        return new Dictionary<string, object?>
        {
            ["Timestamp"] = Timestamp,
            ["CorrelationId"] = CorrelationId,
            ["TenantCode"] = TenantInfo.Code,
            ["TenantName"] = TenantInfo.Name,
            ["ExecutionUser"] = ExecutionUser,
            ["ExecutionOrigin"] = ExecutionOrigin,
            ["BusinessOperationCode"] = BusinessOperationCode
        };
    }

    // Private Methods
    private static bool CanAddMessage(MessageType messageType, MessageType minimumMessageType)
    {
        return messageType >= minimumMessageType;
    }
    private void AddMessage(Message message)
    {
        _ = _messageCollection.TryAdd(message.Id, message);
    }
}
