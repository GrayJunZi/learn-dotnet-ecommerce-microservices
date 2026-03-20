namespace Catalog.Core.Specification;

public class Pagination<T> where T : class
{
    public Pagination()
    {
        
    }

    public Pagination(int pageIndex, int pageSize, long count, IReadOnlyCollection<T> data)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
        Count = count;
        Data = data;
    }

    public IReadOnlyCollection<T> Data { get; set; }

    public long Count { get; set; }

    public int PageSize { get; set; }

    public int PageIndex { get; set; }
}