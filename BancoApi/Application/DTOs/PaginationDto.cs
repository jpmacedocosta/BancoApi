using System.ComponentModel.DataAnnotations;

namespace BancoApi.Application.DTOs
{
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int TotalItems { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
    }

    public class PaginationRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "A página deve ser maior que 0")]
        public int Page { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "O tamanho da página deve estar entre 1 e 100")]
        public int PageSize { get; set; } = 10;
    }
}
