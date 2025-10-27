using BancoApi.Application.DTOs;

namespace BancoApi.Application.Services
{
    public interface IContaService
    {
        Task<ContaDto?> GetContaByIdAsync(int id);
        Task<PagedResult<ContaDto>> GetContaByNomeOrDocumentoPaginatedAsync(string termo, int page, int pageSize);
        Task<ContaDto?> GetContaByDocumentoAsync(string documento);
        Task<ContaDto> CreateContaAsync(CreateContaDto createContaDto);
        Task<ContaDto?> InativarContaAsync(string documento);
    }
}
