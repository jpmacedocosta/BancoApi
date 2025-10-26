using BancoApi.Application.DTOs;

namespace BancoApi.Application.Services
{
    public interface IContaService
    {
        Task<ContaDto?> GetContaByIdAsync(int id);
        Task<IEnumerable<ContaDto>> GetContaByNomeOrDocumentoAsync(string termo);
        Task<ContaDto?> GetContaByDocumentoAsync(string documento);
        Task<ContaDto> CreateContaAsync(CreateContaDto createContaDto);
        Task<ContaDto?> InativarContaAsync(string documento);
    }
}
