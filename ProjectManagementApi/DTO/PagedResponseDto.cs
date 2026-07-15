namespace ProjectManagementApi.DTO
{
    public class PagedResponseDto<T>
    {
        public required IReadOnlyList<T> Items { get; set; }
        public required int Total { get; set; }
        public required int Page { get; set; }
        public required int PageSize { get; set; }
        public required int TotalPages { get; set; }
    }
}
