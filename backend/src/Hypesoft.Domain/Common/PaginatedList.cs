using Microsoft.EntityFrameworkCore;

namespace Hypesoft.Domain.Common;

/// <summary>
/// Represents a paginated list of items.
/// </summary>
/// <typeparam name="T">The type of elements in the list.</typeparam>
public class PaginatedList<T>
{
    /// <summary>
    /// Gets the items in the current page.
    /// </summary>
    public List<T> Items { get; }
    
    /// <summary>
    /// Gets the current page number.
    /// </summary>
    public int PageNumber { get; }
    
    /// <summary>
    /// Gets the total number of pages.
    /// </summary>
    public int TotalPages { get; }
    
    /// <summary>
    /// Gets the total count of items across all pages.
    /// </summary>
    public int TotalCount { get; }
    
    /// <summary>
    /// Gets the number of items per page.
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    /// Gets a value indicating whether there is a previous page.
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;
    
    /// <summary>
    /// Gets a value indicating whether there is a next page.
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Initializes a new instance of the <see cref="PaginatedList{T}"/> class.
    /// </summary>
    /// <param name="items">The items in the current page.</param>
    /// <param name="count">The total count of items across all pages.</param>
    /// <param name="pageNumber">The current page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    public PaginatedList(List<T> items, int count, int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        TotalCount = count;
        Items = items;
    }

    /// <summary>
    /// Creates a new <see cref="PaginatedList{T}"/> asynchronously from the specified data source.
    /// </summary>
    /// <param name="source">The data source.</param>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the paginated list.</returns>
    public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var count = await source.CountAsync(cancellationToken);
        var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new PaginatedList<T>(items, count, pageNumber, pageSize);
    }

    /// <summary>
    /// Creates a new <see cref="PaginatedList{T}"/> from the specified data source.
    /// </summary>
    /// <param name="source">The data source.</param>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <returns>The paginated list.</returns>
    public static PaginatedList<T> Create(IEnumerable<T> source, int pageNumber, int pageSize)
    {
        var enumerable = source as T[] ?? source.ToArray();
        var count = enumerable.Length;
        var items = enumerable.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        return new PaginatedList<T>(items, count, pageNumber, pageSize);
    }

    /// <summary>
    /// Creates a new <see cref="PaginatedList{T}"/> from the specified items and count.
    /// </summary>
    /// <param name="items">The items in the current page.</param>
    /// <param name="totalCount">The total count of items across all pages.</param>
    /// <param name="pageNumber">The current page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>The paginated list.</returns>
    public static PaginatedList<T> Create(List<T> items, int totalCount, int pageNumber, int pageSize)
    {
        return new PaginatedList<T>(items, totalCount, pageNumber, pageSize);
    }
}
