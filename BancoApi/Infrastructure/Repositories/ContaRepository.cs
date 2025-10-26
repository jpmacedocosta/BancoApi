using BancoApi.Domain.Entities;
using BancoApi.Domain.Interfaces;
using BancoApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;

namespace BancoApi.Infrastructure.Repositories
{
    public class ContaRepository : IContaRepository
    {
        private readonly BancoDbContext _context;

        public ContaRepository(BancoDbContext context)
        {
            _context = context;
        }

        public async Task<Conta?> GetByIdAsync(int id)
        {
            return await _context.Contas
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Conta>> GetByNomeOrDocumentoAsync(string termoLimpo)
        {
            var resultados = new List<Conta>();

            var contasPorDocumento = await _context.Contas
                .Where(c => c.Documento == termoLimpo)
                .ToListAsync();
            
            resultados.AddRange(contasPorDocumento);

            var termoNormalizado = RemoverAcentos(termoLimpo);
            var todasContas = await _context.Contas.ToListAsync();
            
            var contasPorNome = todasContas
                .Where(c => RemoverAcentos(c.Nome.Replace(" ", "").ToLower()).Contains(termoNormalizado))
                .ToList();
            
            resultados.AddRange(contasPorNome);

            return resultados.Distinct().OrderBy(x => x.Id).ToList();
        }

        public async Task<Conta?> GetByDocumentoAsync(string documento)
        {
            return await _context.Contas
                .FirstOrDefaultAsync(c => c.Documento == documento);
        }

        public async Task<Conta> CreateAsync(Conta conta)
        {
            _context.Contas.Add(conta);
            await _context.SaveChangesAsync();
            return conta;
        }

        public async Task<Conta> UpdateAsync(Conta conta)
        {
            _context.Entry(conta).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return conta;
        }

        public async Task<bool> ExistsByNumeroAsync(string numero)
        {
            return await _context.Contas
                .AnyAsync(c => c.Numero == numero);
        }

        /// <summary>
        /// Remove acentos de uma string para facilitar a busca
        /// </summary>
        /// <param name="texto">Texto com possíveis acentos</param>
        /// <returns>Texto sem acentos</returns>
        private static string RemoverAcentos(string texto)
        {
            if (string.IsNullOrEmpty(texto))
                return texto;

            var normalizedString = texto.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
