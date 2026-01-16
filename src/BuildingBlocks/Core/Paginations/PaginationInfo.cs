using Bedrock.BuildingBlocks.Core.Filterings;
using Bedrock.BuildingBlocks.Core.Sortings;

namespace Bedrock.BuildingBlocks.Core.Paginations;

/// <summary>
/// Representa informações de paginação para consultas, incluindo ordenação e filtros opcionais.
/// </summary>
/// <remarks>
/// CARACTERÍSTICAS:
/// - Imutável: readonly struct para performance e segurança
/// - Validado: Página e tamanho devem ser maiores que zero
/// - Calculado: Index e Offset são derivados automaticamente
/// - Flexível: Ordenação e filtros são opcionais
///
/// PROPRIEDADES OBRIGATÓRIAS:
/// - Page: Número da página (1-indexed, mínimo 1)
/// - PageSize: Quantidade de itens por página (mínimo 1)
/// - Index: Índice base-zero da página (Page - 1)
/// - Offset: Quantidade de itens a pular (Index * PageSize)
///
/// PROPRIEDADES OPCIONAIS:
/// - SortCollection: Lista de ordenações (pode ser vazia ou null)
/// - FilterCollection: Lista de filtros (pode ser vazia ou null)
///
/// EXEMPLO DE USO (apenas paginação):
///   var pagination = PaginationInfo.Create(page: 3, pageSize: 10);
///
/// EXEMPLO DE USO (com ordenação e filtros):
///   var sortCollection = new[] { SortInfo.Create("LastName", SortDirection.Ascending) };
///   var filterCollection = new[] { FilterInfo.Create("Status", FilterOperator.Equals, "Active") };
///   var pagination = PaginationInfo.Create(page: 1, pageSize: 20, sortCollection, filterCollection);
///
/// EXEMPLO DE USO (todos os registros):
///   var allItems = PaginationInfo.All;
///   var allSorted = PaginationInfo.All.WithSortCollection(sorts);
///   var allFiltered = PaginationInfo.CreateAll(sorts, filters);
///
/// USO COM LINQ:
///   var items = query
///       .Where(/* aplicar filterCollection */)
///       .OrderBy(/* aplicar sortCollection */)
///       .Skip(pagination.Offset)
///       .Take(pagination.PageSize)
///       .ToList();
///
/// SEGURANÇA:
/// - Os campos em Sorts e Filters devem ser validados contra uma whitelist
///   na camada Infra.Data antes de serem usados em queries
/// </remarks>
public readonly struct PaginationInfo : IEquatable<PaginationInfo>
{
    /// <summary>
    /// Número da página (1-indexed).
    /// Sempre maior que zero.
    /// </summary>
    public int Page { get; }

    /// <summary>
    /// Quantidade de itens por página.
    /// Sempre maior que zero.
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    /// Índice base-zero da página.
    /// Calculado como (Page - 1).
    /// </summary>
    /// <remarks>
    /// Útil para APIs que usam índice base-zero ao invés de número de página.
    /// Exemplo: Page 1 → Index 0, Page 2 → Index 1, Page 3 → Index 2
    /// </remarks>
    public int Index => Page - 1;

    /// <summary>
    /// Quantidade de itens a pular para chegar na página atual.
    /// Calculado como (Index * PageSize) ou ((Page - 1) * PageSize).
    /// </summary>
    /// <remarks>
    /// Usado diretamente com LINQ Skip() ou SQL OFFSET.
    /// Exemplo com PageSize=10:
    ///   Page 1 → Offset 0 (não pula nada)
    ///   Page 2 → Offset 10 (pula 10 itens)
    ///   Page 3 → Offset 20 (pula 20 itens)
    /// </remarks>
    public int Offset => Index * PageSize;

    /// <summary>
    /// Lista de ordenações a aplicar (opcional).
    /// </summary>
    /// <remarks>
    /// A ordem dos itens define a prioridade da ordenação.
    /// Exemplo: [LastName ASC, FirstName ASC] → ORDER BY LastName ASC, FirstName ASC
    ///
    /// SEGURANÇA: Os campos devem ser validados contra uma whitelist na camada Infra.Data.
    /// </remarks>
    public IReadOnlyList<SortInfo>? SortCollection { get; }

    /// <summary>
    /// Lista de filtros a aplicar (opcional).
    /// </summary>
    /// <remarks>
    /// Filtros são combinados com AND por padrão.
    ///
    /// SEGURANÇA: Os campos devem ser validados contra uma whitelist na camada Infra.Data.
    /// </remarks>
    public IReadOnlyList<FilterInfo>? FilterCollection { get; }

    /// <summary>
    /// Indica se há ordenações definidas.
    /// </summary>
    public bool HasSort => SortCollection is not null && SortCollection.Count > 0;

    /// <summary>
    /// Indica se há filtros definidos.
    /// </summary>
    public bool HasFilter => FilterCollection is not null && FilterCollection.Count > 0;

    /// <summary>
    /// Indica se esta instância representa uma consulta sem limite de registros.
    /// </summary>
    /// <remarks>
    /// Verdadeiro quando PageSize é int.MaxValue, indicando que o cliente
    /// deseja todos os registros (com filtros/ordenação opcionais).
    ///
    /// ATENÇÃO: Queries unbounded podem retornar grandes volumes de dados.
    /// A camada Infra.Data pode impor limites adicionais se necessário.
    /// </remarks>
    public bool IsUnbounded => PageSize == int.MaxValue;

    private PaginationInfo(
        int page,
        int pageSize,
        IReadOnlyList<SortInfo>? sortCollection,
        IReadOnlyList<FilterInfo>? filterCollection)
    {
        Page = page;
        PageSize = pageSize;
        SortCollection = sortCollection;
        FilterCollection = filterCollection;
    }

    /// <summary>
    /// Retorna uma instância de PaginationInfo que representa todos os registros.
    /// </summary>
    /// <remarks>
    /// Use esta propriedade quando precisar recuperar todos os registros sem paginação.
    /// Pode ser combinada com WithSortCollection() e WithFilterCollection() para
    /// adicionar ordenação e filtros.
    ///
    /// EXEMPLO DE USO:
    ///   var allItems = PaginationInfo.All;
    ///   var allSorted = PaginationInfo.All.WithSortCollection(sorts);
    ///
    /// ATENÇÃO: Use com cuidado em coleções grandes para evitar problemas de memória.
    /// </remarks>
    public static PaginationInfo All => CreateAll();

    /// <summary>
    /// Cria uma instância de PaginationInfo para recuperar todos os registros,
    /// opcionalmente com ordenação e filtros.
    /// </summary>
    /// <param name="sortCollection">Lista de ordenações (opcional).</param>
    /// <param name="filterCollection">Lista de filtros (opcional).</param>
    /// <returns>PaginationInfo configurado para retornar todos os registros.</returns>
    /// <remarks>
    /// EXEMPLO DE USO:
    ///   var allFiltered = PaginationInfo.CreateAll(
    ///       sortCollection: new[] { SortInfo.Create("Name", SortDirection.Ascending) },
    ///       filterCollection: new[] { FilterInfo.Create("Status", FilterOperator.Equals, "Active") });
    ///
    /// ATENÇÃO: Use com cuidado em coleções grandes para evitar problemas de memória.
    /// </remarks>
    public static PaginationInfo CreateAll(
        IReadOnlyList<SortInfo>? sortCollection = null,
        IReadOnlyList<FilterInfo>? filterCollection = null)
    {
        return new PaginationInfo(1, int.MaxValue, sortCollection, filterCollection);
    }

    /// <summary>
    /// Cria uma nova instância de PaginationInfo apenas com paginação.
    /// </summary>
    /// <param name="page">Número da página (deve ser maior que zero).</param>
    /// <param name="pageSize">Tamanho da página (deve ser maior que zero).</param>
    /// <returns>Nova instância de PaginationInfo.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Lançada quando page ou pageSize são menores ou iguais a zero.
    /// </exception>
    public static PaginationInfo Create(int page, int pageSize)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(page, 0, nameof(page));
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(pageSize, 0, nameof(pageSize));

        return new PaginationInfo(page, pageSize, null, null);
    }

    /// <summary>
    /// Cria uma nova instância de PaginationInfo com paginação, ordenação e filtros.
    /// </summary>
    /// <param name="page">Número da página (deve ser maior que zero).</param>
    /// <param name="pageSize">Tamanho da página (deve ser maior que zero).</param>
    /// <param name="sortCollection">Lista de ordenações (opcional).</param>
    /// <param name="filterCollection">Lista de filtros (opcional).</param>
    /// <returns>Nova instância de PaginationInfo.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Lançada quando page ou pageSize são menores ou iguais a zero.
    /// </exception>
    public static PaginationInfo Create(
        int page,
        int pageSize,
        IReadOnlyList<SortInfo>? sortCollection,
        IReadOnlyList<FilterInfo>? filterCollection)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(page, 0, nameof(page));
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(pageSize, 0, nameof(pageSize));

        return new PaginationInfo(page, pageSize, sortCollection, filterCollection);
    }

    /// <summary>
    /// Cria uma instância de PaginationInfo a partir de valores existentes sem validação.
    /// </summary>
    /// <param name="page">Número da página.</param>
    /// <param name="pageSize">Tamanho da página.</param>
    /// <param name="sortCollection">Lista de ordenações (opcional).</param>
    /// <param name="filterCollection">Lista de filtros (opcional).</param>
    /// <returns>Nova instância de PaginationInfo.</returns>
    /// <remarks>
    /// ATENÇÃO: Este método NÃO valida os parâmetros.
    /// Use apenas para reconstruir PaginationInfo a partir de valores conhecidos/armazenados.
    /// Para criar novas instâncias, use Create().
    /// </remarks>
    public static PaginationInfo CreateFromExistingInfo(
        int page,
        int pageSize,
        IReadOnlyList<SortInfo>? sortCollection,
        IReadOnlyList<FilterInfo>? filterCollection)
    {
        return new PaginationInfo(page, pageSize, sortCollection, filterCollection);
    }

    /// <summary>
    /// Cria uma nova instância com ordenações adicionais.
    /// </summary>
    /// <param name="sortCollection">Lista de ordenações a adicionar.</param>
    /// <returns>Nova instância de PaginationInfo com as ordenações.</returns>
    public PaginationInfo WithSortCollection(IReadOnlyList<SortInfo> sortCollection)
    {
        return new PaginationInfo(Page, PageSize, sortCollection, FilterCollection);
    }

    /// <summary>
    /// Cria uma nova instância com filtros adicionais.
    /// </summary>
    /// <param name="filterCollection">Lista de filtros a adicionar.</param>
    /// <returns>Nova instância de PaginationInfo com os filtros.</returns>
    public PaginationInfo WithFilterCollection(IReadOnlyList<FilterInfo> filterCollection)
    {
        return new PaginationInfo(Page, PageSize, SortCollection, filterCollection);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Page, PageSize);
    }

    public override bool Equals(object? obj)
    {
        return obj is PaginationInfo other && Equals(other);
    }

    public bool Equals(PaginationInfo other)
    {
        return Page == other.Page && PageSize == other.PageSize;
    }

    public override string ToString()
    {
        var parts = new List<string>
        {
            $"Page: {Page}",
            $"PageSize: {PageSize}",
            $"Index: {Index}",
            $"Offset: {Offset}"
        };

        if (HasSort)
        {
            parts.Add($"SortCollection: [{string.Join(", ", SortCollection!)}]");
        }

        if (HasFilter)
        {
            parts.Add($"FilterCollection: [{string.Join(", ", FilterCollection!)}]");
        }

        return string.Join(", ", parts);
    }

    public static bool operator ==(PaginationInfo left, PaginationInfo right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(PaginationInfo left, PaginationInfo right)
    {
        return !left.Equals(right);
    }
}
