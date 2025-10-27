using BancoApi.Domain.Entities;

namespace BancoApi.Domain.Interfaces
{
    public interface IContaRepository
    {
        Task<List<Conta>> GetByNomeOrDocumentoAsync(string termo);
        Task<Conta?> GetByDocumentoAsync(string documento);
        Task<Conta> CreateAsync(Conta conta);
        Task<Conta> UpdateAsync(Conta conta);
        Task<bool> ExistsByNumeroAsync(string numero);
    }
}
