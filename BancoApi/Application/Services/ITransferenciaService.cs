using BancoApi.Application.DTOs;

namespace BancoApi.Application.Services
{
    public interface ITransferenciaService
    {
        Task<TransferenciaDto?> GetTransferenciaByIdAsync(int id);
        Task<IEnumerable<TransferenciaDto>> GetTransferenciasPorContaAsync(string documento);
        Task<TransferenciaDto> CreateTransferenciaAsync(CreateTransferenciaDto createDto);
    }
}
