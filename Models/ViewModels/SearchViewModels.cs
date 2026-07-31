namespace HMS.Models.ViewModels;

public class SearchHit
{
    public string Kind { get; set; } = "";      // "Dish", "Product", "Order", "Table", "Staff"
    public string Icon { get; set; } = "";
    public string Title { get; set; } = "";
    public string Meta { get; set; } = "";      // price, stock, status — one short line
    public string Controller { get; set; } = "";
    public string Action { get; set; } = "Index";
    public int? Id { get; set; }
}

public class SearchResultsViewModel
{
    public string Query { get; set; } = "";
    public List<SearchHit> Hits { get; set; } = [];

    /// <summary>Groups in a stable order so results do not reshuffle between queries.</summary>
    public IEnumerable<IGrouping<string, SearchHit>> Groups =>
        Hits.GroupBy(h => h.Kind).OrderBy(g => g.Key);
}
