namespace FlightBoard.Api.iFX.Utilities.Helpers;

/// <summary>
/// Helper class for pagination operations
/// </summary>
public static class PaginationHelper
{
    /// <summary>
    /// Calculate total pages based on total count and page size
    /// </summary>
    public static int CalculateTotalPages(int totalCount, int pageSize)
    {
        if (pageSize <= 0) return 0;
        return (int)Math.Ceiling((double)totalCount / pageSize);
    }

    /// <summary>
    /// Calculate skip count for pagination
    /// </summary>
    public static int CalculateSkipCount(int page, int pageSize)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 20;

        return (page - 1) * pageSize;
    }

    /// <summary>
    /// Validate pagination parameters
    /// </summary>
    public static (int validPage, int validPageSize) ValidatePaginationParams(int page, int pageSize, int maxPageSize = 100)
    {
        var validPage = Math.Max(1, page);
        var validPageSize = Math.Min(Math.Max(1, pageSize), maxPageSize);

        return (validPage, validPageSize);
    }

    /// <summary>
    /// Check if there's a next page
    /// </summary>
    public static bool HasNextPage(int currentPage, int totalPages)
    {
        return currentPage < totalPages;
    }

    /// <summary>
    /// Check if there's a previous page
    /// </summary>
    public static bool HasPreviousPage(int currentPage)
    {
        return currentPage > 1;
    }

    /// <summary>
    /// Get current page item range (e.g., "1-20 of 150")
    /// </summary>
    public static (int startItem, int endItem) GetItemRange(int page, int pageSize, int totalCount)
    {
        if (totalCount == 0) return (0, 0);

        var startItem = ((page - 1) * pageSize) + 1;
        var endItem = Math.Min(startItem + pageSize - 1, totalCount);

        return (startItem, endItem);
    }
}
