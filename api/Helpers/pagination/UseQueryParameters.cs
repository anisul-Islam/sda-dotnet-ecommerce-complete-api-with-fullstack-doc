public class QueryParameters
{
  public int PageNumber { get; set; } = 1;
  public int PageSize { get; set; } = 5;
  public string? SearchTerm { get; set; } = "";
  public string? SortBy { get; set; } = "Name"; // Default sorting by name
  public string? SortOrder { get; set; } = "asc"; // asc or desc
  public List<Guid>? SelectedCategories { get; set; } = new List<Guid>();

  public decimal? MinPrice { get; set; }
  public decimal? MaxPrice { get; set; }

}
