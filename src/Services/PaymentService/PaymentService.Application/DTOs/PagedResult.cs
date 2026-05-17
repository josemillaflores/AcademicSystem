namespace PaymentService.Application.DTOs;

/// <summary>
/// Resultado paginado para respuestas de API
/// </summary>
/// <typeparam name="T">Tipo de los elementos en la página</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// Elementos de la página actual
    /// </summary>
    public IEnumerable<T> Items { get; set; } = new List<T>();
    
    /// <summary>
    /// Número de página actual (1-indexado)
    /// </summary>
    public int Page { get; set; }
    
    /// <summary>
    /// Tamaño de página
    /// </summary>
    public int PageSize { get; set; }
    
    /// <summary>
    /// Total de elementos en la base de datos
    /// </summary>
    public int TotalCount { get; set; }
    
    /// <summary>
    /// Total de páginas
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    
    /// <summary>
    /// Indica si existe página anterior
    /// </summary>
    public bool HasPreviousPage => Page > 1;
    
    /// <summary>
    /// Indica si existe página siguiente
    /// </summary>
    public bool HasNextPage => Page < TotalPages;
    
    public PagedResult() { }
    
    public PagedResult(IEnumerable<T> items, int totalCount, int page, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
    }
}
