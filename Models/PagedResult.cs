namespace DemoMVC.Models
{
    public class PagedResult<T>
    {
        public required IReadOnlyList<T> Items { get; init; }
        public int TotalCount { get; init; }
        public int Page { get; init; }
        public int PageSize { get; init; }
        public int TotalPages { get; init; }
        public string Search { get; init; } = string.Empty;
    }
}
