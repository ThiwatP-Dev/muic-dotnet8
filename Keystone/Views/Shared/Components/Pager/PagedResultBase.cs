using KeystoneLibrary.Models;

public abstract class PagedResultBase
{
    public int CurrentPage { get; set; }
    public int PageCount { get; set; }
    public int PageSize { get; set; }
    public int RowCount { get; set; }

    public int FirstRowOnPage
    {
        get 
        { 
            return (CurrentPage > 0 ? CurrentPage - 1 : 0) * PageSize + 1; 
        }
    }

    public int LastRowOnPage
    {
        get 
        { 
            return Math.Min(CurrentPage * PageSize, RowCount); 
        }
    }
}

public class PagedResult<T> : PagedResultBase where T : class
{
    public IList<T> Results { get; set; }

    public Criteria Criteria { get; set; }

    public PagedResult()
    {
        Results = new List<T>();
        Criteria = new Criteria();
    }
}