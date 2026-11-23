namespace BroomErp.Application.Common.Models;

public class PagedResult<T>
{
    public List<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class QueryParams
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string? Search { get; set; }
    public string? Sort { get; set; }
    public string Order { get; set; } = "desc";
    public bool? Active { get; set; }
    public string? Status { get; set; }
}
