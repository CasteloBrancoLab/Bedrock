namespace Bedrock.BuildingBlocks.Core.Filterings.Enums;

/// <summary>
/// Operadores de filtro suportados.
/// </summary>
/// <remarks>
/// CATEGORIAS DE OPERADORES:
///
/// IGUALDADE:
/// - Equals: Valor exato
/// - NotEquals: Diferente de
///
/// TEXTO (strings):
/// - Contains: Contém substring
/// - StartsWith: Começa com
/// - EndsWith: Termina com
///
/// COMPARAÇÃO (números, datas):
/// - GreaterThan: Maior que
/// - GreaterThanOrEquals: Maior ou igual
/// - LessThan: Menor que
/// - LessThanOrEquals: Menor ou igual
///
/// INTERVALO:
/// - Between: Entre dois valores (requer Value e ValueEnd)
///
/// LISTA:
/// - In: Está na lista (requer Values array)
/// - NotIn: Não está na lista (requer Values array)
/// </remarks>
public enum FilterOperator
{
    /// <summary>
    /// Valor exato (=).
    /// </summary>
    Equals = 1,

    /// <summary>
    /// Diferente de (!=).
    /// </summary>
    NotEquals = 2,

    /// <summary>
    /// Contém substring (LIKE '%value%').
    /// Aplicável apenas a strings.
    /// </summary>
    Contains = 3,

    /// <summary>
    /// Começa com (LIKE 'value%').
    /// Aplicável apenas a strings.
    /// </summary>
    StartsWith = 4,

    /// <summary>
    /// Termina com (LIKE '%value').
    /// Aplicável apenas a strings.
    /// </summary>
    EndsWith = 5,

    /// <summary>
    /// Maior que (>).
    /// Aplicável a números e datas.
    /// </summary>
    GreaterThan = 6,

    /// <summary>
    /// Maior ou igual (>=).
    /// Aplicável a números e datas.
    /// </summary>
    GreaterThanOrEquals = 7,

    /// <summary>
    /// Menor que (&lt;).
    /// Aplicável a números e datas.
    /// </summary>
    LessThan = 8,

    /// <summary>
    /// Menor ou igual (&lt;=).
    /// Aplicável a números e datas.
    /// </summary>
    LessThanOrEquals = 9,

    /// <summary>
    /// Entre dois valores (BETWEEN).
    /// Requer Value (início) e ValueEnd (fim).
    /// Aplicável a números e datas.
    /// </summary>
    Between = 10,

    /// <summary>
    /// Está na lista (IN).
    /// Requer Values array.
    /// </summary>
    In = 11,

    /// <summary>
    /// Não está na lista (NOT IN).
    /// Requer Values array.
    /// </summary>
    NotIn = 12
}
