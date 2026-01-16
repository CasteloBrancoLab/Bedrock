using Bedrock.BuildingBlocks.Core.Validations.Enums;
using ExecutionContext = Bedrock.BuildingBlocks.Core.ExecutionContexts.ExecutionContext;

namespace Bedrock.BuildingBlocks.Core.Validations;

/// <summary>
/// Utilitários para validações comuns com integração ao ExecutionContext.
/// </summary>
/// <remarks>
/// PADRÃO DE USO:
/// Os métodos de validação retornam bool indicando se a validação passou.
/// Em caso de falha, uma mensagem de erro é adicionada ao ExecutionContext
/// no formato: {propertyName}.{ValidationType}
///
/// EXEMPLO:
/// <code>
/// bool isValid = true;
/// isValid &amp;= ValidationUtils.ValidateIsRequired(context, "Email", true, email);
/// isValid &amp;= ValidationUtils.ValidateMinLength(context, "Age", 18, age);
/// isValid &amp;= ValidationUtils.ValidateMaxLength(context, "Name", 100, name?.Length);
///
/// if (!isValid)
///     return Result.Failure("Validation failed");
/// </code>
///
/// NOTA SOBRE NULL:
/// - ValidateIsRequired: null é considerado inválido quando isRequired=true
/// - ValidateMinLength/MaxLength: null é considerado válido (use IsRequired para obrigatoriedade)
/// </remarks>
public static class ValidationUtils
{
    /// <summary>
    /// Valida se um valor obrigatório está preenchido.
    /// </summary>
    /// <typeparam name="TValue">Tipo do valor a validar.</typeparam>
    /// <param name="executionContext">Contexto para adicionar mensagem de erro.</param>
    /// <param name="propertyName">Nome da propriedade (para mensagem de erro).</param>
    /// <param name="isRequired">Se true, o valor é obrigatório.</param>
    /// <param name="value">Valor a validar.</param>
    /// <returns>true se válido, false se inválido.</returns>
    /// <remarks>
    /// Um valor é considerado inválido quando:
    /// - isRequired é true E (value é null OU value é igual ao default do tipo)
    ///
    /// Exemplos de valores inválidos (quando isRequired=true):
    /// - null para qualquer tipo
    /// - 0 para int
    /// - Guid.Empty para Guid
    /// - false para bool
    /// - "" para string (default é null, mas "" também falha pois Equals(default))
    /// </remarks>
    public static bool ValidateIsRequired<TValue>(
        ExecutionContext executionContext,
        string propertyName,
        bool isRequired,
        TValue? value)
    {
        if (isRequired && (value is null || value.Equals(default(TValue))))
        {
            executionContext.AddErrorMessage(
                code: $"{propertyName}.{ValidationType.IsRequired}");

            return false;
        }

        return true;
    }

    /// <summary>
    /// Valida se um valor é maior ou igual ao mínimo especificado.
    /// </summary>
    /// <typeparam name="TValue">Tipo comparável do valor.</typeparam>
    /// <param name="executionContext">Contexto para adicionar mensagem de erro.</param>
    /// <param name="propertyName">Nome da propriedade (para mensagem de erro).</param>
    /// <param name="minLength">Valor mínimo permitido.</param>
    /// <param name="value">Valor a validar.</param>
    /// <returns>true se válido, false se inválido.</returns>
    /// <remarks>
    /// Valores nulos são considerados válidos.
    /// Use ValidateIsRequired separadamente para obrigatoriedade.
    ///
    /// Exemplo:
    /// <code>
    /// // Valida idade mínima de 18 anos
    /// ValidationUtils.ValidateMinLength(context, "Age", 18, person.Age);
    ///
    /// // Valida tamanho mínimo de string
    /// ValidationUtils.ValidateMinLength(context, "Name", 3, name?.Length);
    /// </code>
    /// </remarks>
    public static bool ValidateMinLength<TValue>(
        ExecutionContext executionContext,
        string propertyName,
        TValue minLength,
        TValue? value)
        where TValue : IComparable<TValue>
    {
        if (value is null)
            return true;

        if (value.CompareTo(minLength) < 0)
        {
            executionContext.AddErrorMessage(
                code: $"{propertyName}.{ValidationType.MinLength}");

            return false;
        }

        return true;
    }

    /// <summary>
    /// Valida se um valor é menor ou igual ao máximo especificado.
    /// </summary>
    /// <typeparam name="TValue">Tipo comparável do valor.</typeparam>
    /// <param name="executionContext">Contexto para adicionar mensagem de erro.</param>
    /// <param name="propertyName">Nome da propriedade (para mensagem de erro).</param>
    /// <param name="maxLength">Valor máximo permitido.</param>
    /// <param name="value">Valor a validar.</param>
    /// <returns>true se válido, false se inválido.</returns>
    /// <remarks>
    /// Valores nulos são considerados válidos.
    /// Use ValidateIsRequired separadamente para obrigatoriedade.
    ///
    /// Exemplo:
    /// <code>
    /// // Valida idade máxima de 120 anos
    /// ValidationUtils.ValidateMaxLength(context, "Age", 120, person.Age);
    ///
    /// // Valida tamanho máximo de string
    /// ValidationUtils.ValidateMaxLength(context, "Name", 100, name?.Length);
    /// </code>
    /// </remarks>
    public static bool ValidateMaxLength<TValue>(
        ExecutionContext executionContext,
        string propertyName,
        TValue maxLength,
        TValue? value)
        where TValue : IComparable<TValue>
    {
        if (value is null)
            return true;

        if (value.CompareTo(maxLength) > 0)
        {
            executionContext.AddErrorMessage(
                code: $"{propertyName}.{ValidationType.MaxLength}");

            return false;
        }

        return true;
    }
}
