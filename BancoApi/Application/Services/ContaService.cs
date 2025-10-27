using BancoApi.Application.DTOs;
using BancoApi.Domain.Entities;
using BancoApi.Domain.Interfaces;

namespace BancoApi.Application.Services
{
    public class ContaService : IContaService
    {
        private readonly IContaRepository _contaRepository;

        public ContaService(IContaRepository contaRepository)
        {
            _contaRepository = contaRepository;
        }

        public async Task<ContaDto?> GetContaByIdAsync(int id)
        {
            var conta = await _contaRepository.GetByIdAsync(id);
            return conta != null ? MapToDto(conta) : null;
        }

        public async Task<PagedResult<ContaDto>> GetContaByNomeOrDocumentoPaginatedAsync(string termo, int page, int pageSize)
        {
            termo = TirarFormatacaoDocumento(termo.Replace(" ", "").ToLower());

            var todasContas = await _contaRepository.GetByNomeOrDocumentoAsync(termo);
            var contasDto = todasContas.Select(MapToDto).ToList();

            var totalItems = contasDto.Count;
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var skip = (page - 1) * pageSize;
            var contasPaginadas = contasDto.Skip(skip).Take(pageSize);

            return new PagedResult<ContaDto>
            {
                Items = contasPaginadas,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        public async Task<ContaDto> CreateContaAsync(CreateContaDto createContaDto)
        {
            var contaExistente = await GetContaByDocumentoAsync(createContaDto.Documento);
            if (contaExistente != null)
            {
                throw new InvalidOperationException("Já existe uma conta com o mesmo número de documento.");
            }

            var numeroContaGerado = await GerarNumeroContaAsync();
            var documentoSemFormatacao = TirarFormatacaoDocumento(createContaDto.Documento);

            var conta = new Conta
            {
                Numero = numeroContaGerado,
                Nome = createContaDto.Nome,
                Documento = documentoSemFormatacao,
                Saldo = 1000.00m, // Saldo inicial padrão
                DataCriacao = DateTime.UtcNow,
                Status = StatusConta.Ativa
            };

            var contaCriada = await _contaRepository.CreateAsync(conta);
            return MapToDto(contaCriada);
        }

        public async Task<ContaDto?> InativarContaAsync(string documento)
        {
            documento = TirarFormatacaoDocumento(documento);

            var conta = await _contaRepository.GetByDocumentoAsync(documento);
            if (conta == null)
            {
                return null;
            }

            if (conta.Status == StatusConta.Inativa)
            {
                throw new InvalidOperationException("A conta informada já está inativa.");
            }

            conta.Status = StatusConta.Inativa;
            conta.DataAlteracao = DateTime.UtcNow;
            conta.UsuarioAlteracao = "Sistema";

            var contaAtualizada = await _contaRepository.UpdateAsync(conta);
            return MapToDto(contaAtualizada);
        }

        public async Task<ContaDto?> GetContaByDocumentoAsync(string documento)
        {
            documento = TirarFormatacaoDocumento(documento);

            var conta = await _contaRepository.GetByDocumentoAsync(documento);
            return conta != null ? MapToDto(conta) : null;
        }

        private static ContaDto MapToDto(Conta conta)
        {
            return new ContaDto
            {
                Id = conta.Id,
                Numero = conta.Numero,
                Nome = conta.Nome,
                Documento = conta.Documento,
                Saldo = conta.Saldo,
                DataCriacao = conta.DataCriacao,
                Status = conta.Status
            };
        }

        private static string TirarFormatacaoDocumento(string documento)
        {
            return documento.Replace(".", "").Replace("-", "").Replace(" ", "");
        }

        private async Task<string> GerarNumeroContaAsync()
        {
            string numeroGerado;
            bool numeroExiste;

            // Gera número aleatório com 5 dígitos + dígito verificador
            do
            {
                var random = new Random();
                var numeroBase = random.Next(10000, 99999);
                var digitoVerificador = numeroBase % 10;
                numeroGerado = $"{numeroBase}{digitoVerificador}";

                numeroExiste = await _contaRepository.ExistsByNumeroAsync(numeroGerado);
            }
            while (numeroExiste);

            return numeroGerado;
        }
    }
}
