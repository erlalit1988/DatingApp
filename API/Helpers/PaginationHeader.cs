namespace API.Helpers
{
    public class PaginationHeader(int currentPage, int itemPerPage, 
        int totalItems, int totalPages)
    {
        public int CurrentPage { get; set; } = currentPage;
        public int ItemPerPage { get; set; } = itemPerPage;
        public int TotalItems { get; set; } = totalItems;
        public int TotalPages { get; set; } = totalPages;

    }
}
