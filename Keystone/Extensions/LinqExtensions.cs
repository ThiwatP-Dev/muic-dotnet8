using KeystoneLibrary.Models;
using Newtonsoft.Json;

public static class LinqExtensions
{
    public static PagedResult<T> GetPaged<T>(this IQueryable<T> query,
                                             int page, bool isReportTable = false, int pageSize = 15) where T : class
    {
        var result = new PagedResult<T>();
        result.CurrentPage = page;
        result.PageSize = pageSize;
        result.RowCount = query.Count();
        if (!isReportTable)
        {
            var pageCount = (double)result.RowCount / pageSize;
            result.PageCount = (int)Math.Ceiling(pageCount);

            var skip = (page - 1) * pageSize;
            if (skip < 0 )
                skip = 0;

            result.Results = query.Skip(skip).Take(pageSize).ToList();
        }
        else
        {
            result.Results = query.ToList();
        }
        return result;
    }

    public static PagedResult<T> GetPaged<T>(this IQueryable<T> query, Criteria criteria,
                                             int page, bool isReportTable = false, int pageSize = 15) where T : class
    {
        var result = new PagedResult<T>();
        result.CurrentPage = page;
        result.PageSize = pageSize;
        result.RowCount = query.Count();
        result.Criteria = criteria;

        if (!isReportTable)
        {
            var pageCount = (double)result.RowCount / pageSize;
            result.PageCount = (int)Math.Ceiling(pageCount);

            var skip = (page - 1) * pageSize;
            if (skip < 0 )
                skip = 0;

            result.Results = query.Skip(skip).Take(pageSize).ToList();
        }
        else
        {
            result.Results = query.ToList();
        }

        return result;
    }

    public static PagedResult<T> GetPaged<T>(this List<T> query, Criteria criteria,
                                             int page, bool isReportTable = false, int pageSize = 15) where T : class
    {
        var result = new PagedResult<T>();
        result.CurrentPage = page;
        result.PageSize = pageSize;
        result.RowCount = query.Count();
        result.Criteria = criteria;

        if (!isReportTable)
        {
            var pageCount = (double)result.RowCount / pageSize;
            result.PageCount = (int)Math.Ceiling(pageCount);

            var skip = (page - 1) * pageSize;
            if (skip < 0 )
                skip = 0;

            result.Results = query.Skip(skip).Take(pageSize).ToList();
        }
        else
        {
            result.Results = query;
        }

        return result;
    }

    public static PagedResult<T> GetAllPaged<T>(this IQueryable<T> query, Criteria criteria) where T : class
    {
        var result = new PagedResult<T>();
        result.RowCount = query.Count();
        result.Criteria = criteria;
            
        result.Results = query.ToList();

        return result;
    }

    public static PagedResult<T> GetAllPaged<T>(this IEnumerable<T> query, Criteria criteria) where T : class
    {
        var result = new PagedResult<T>();
        result.RowCount = query.Count();
        result.Criteria = criteria;
            
        result.Results = query.ToList();

        return result;
    }

    public static string SerializeMultiple<T>(this List<T> list)
    {
        return list == null || !list.Any() ? "[]" : JsonConvert.SerializeObject(list);
    }
}