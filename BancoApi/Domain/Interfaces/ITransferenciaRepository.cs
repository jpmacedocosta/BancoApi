using BancoApi.Domain.Entities;

namespace BancoApi.Domain.Interfaces
{
    public interface ITransferenciaRepository
    {
        Task<Transferencia?> GetByIdAsync(int id);
        Task<IEnumerable<Transferencia>> GetByDocumentoAsync(string documento);
        Task<Transferencia> CreateAsync(Transferencia transferencia);
    }
}
