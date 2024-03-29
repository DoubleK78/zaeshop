namespace Common.Models
{
    public class PagingCommonRequest
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
        public string? SortColumn { get; set; }
        public string? SortDirection { get; set; }
    }

    public class PagingCommonResponse<T>
    {
        public long? RowNum { get; set; }
        public List<T>? Data { get; set; }
    }
}
