using BancoApi.Domain.Entities;
using BancoApi.Domain.Interfaces;
using BancoApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BancoApi.Infrastructure.Repositories
{
    public class TransferenciaRepository : ITransferenciaRepository
    {
        private readonly BancoDbContext _context;

        public TransferenciaRepository(BancoDbContext context)
        {
            _context = context;
        }

        public async Task<Transferencia?> GetByIdAsync(int id)
        {
            return await _context.Transferencias
                .Include(t => t.ContaOrigem)
                .Include(t => t.ContaDestino)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Transferencia>> GetByDocumentoAsync(string documento)
        {
            return await _context.Transferencias
                .Include(t => t.ContaOrigem)
                .Include(t => t.ContaDestino)
                .Where(t => t.ContaOrigem.Documento == documento || t.ContaDestino.Documento == documento)
                .OrderByDescending(t => t.DataTransferencia)
                .ToListAsync();
        }

        public async Task<Transferencia> CreateAsync(Transferencia transferencia)
        {
            _context.Transferencias.Add(transferencia);
            await _context.SaveChangesAsync();
            
            return await GetByIdAsync(transferencia.Id) ?? transferencia;
        }
    }
}
