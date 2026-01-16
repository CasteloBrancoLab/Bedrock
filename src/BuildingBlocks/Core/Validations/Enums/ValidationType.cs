namespace Bedrock.BuildingBlocks.Core.Validations.Enums;

/// <summary>
/// Tipos de validação suportados pelo ValidationUtils.
/// </summary>
/// <remarks>
/// Usados para gerar códigos de erro padronizados no formato:
/// {PropertyName}.{ValidationType}
///
/// Exemplos:
/// - Email.IsRequired
/// - Age.MinLength
/// - Name.MaxLength
/// </remarks>
public enum ValidationType
{
    /// <summary>
    /// Validação de campo obrigatório.
    /// Falha quando valor é null ou default.
    /// </summary>
    IsRequired = 1,

    /// <summary>
    /// Validação de valor mínimo.
    /// Falha quando valor é menor que o mínimo especificado.
    /// </summary>
    MinLength = 2,

    /// <summary>
    /// Validação de valor máximo.
    /// Falha quando valor é maior que o máximo especificado.
    /// </summary>
    MaxLength = 3
}
