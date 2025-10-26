using BancoApi.Application.DTOs;
using BancoApi.Domain.Entities;
using BancoApi.Domain.Interfaces;

namespace BancoApi.Application.Services
{
    public class TransferenciaService : ITransferenciaService
    {
        private readonly ITransferenciaRepository _transferenciaRepository;
        private readonly IContaRepository _contaRepository;

        public TransferenciaService(ITransferenciaRepository transferenciaRepository, IContaRepository contaRepository)
        {
            _transferenciaRepository = transferenciaRepository;
            _contaRepository = contaRepository;
        }

        public async Task<TransferenciaDto?> GetTransferenciaByIdAsync(int id)
        {
            var transferencia = await _transferenciaRepository.GetByIdAsync(id);
            return transferencia != null ? MapToDto(transferencia) : null;
        }

        public async Task<IEnumerable<TransferenciaDto>> GetTransferenciasPorContaAsync(string documento)
        {
            var transferencias = await _transferenciaRepository.GetByDocumentoAsync(documento);
            return transferencias.Select(MapToDto);
        }

        public async Task<TransferenciaDto> CreateTransferenciaAsync(CreateTransferenciaDto createDto)
        {
            var documentoOrigem = TirarFormatacaoDocumento(createDto.DocumentoContaOrigem);
            var documentoDestino = TirarFormatacaoDocumento(createDto.DocumentoContaDestino);

            var contaOrigem = await _contaRepository.GetByDocumentoAsync(documentoOrigem)
                ?? throw new InvalidOperationException("Conta de origem não encontrada.");
                
            var contaDestino = await _contaRepository.GetByDocumentoAsync(documentoDestino) 
                ?? throw new InvalidOperationException("Conta de destino não encontrada.");

            if (contaOrigem.Id == contaDestino.Id)
            {
                throw new InvalidOperationException("Conta de origem e destino não podem ser iguais.");
            }

            if (contaOrigem.Status != StatusConta.Ativa)
            {
                throw new InvalidOperationException("Conta de origem deve estar ativa.");
            }

            if (contaDestino.Status != StatusConta.Ativa)
            {
                throw new InvalidOperationException("Conta de destino deve estar ativa.");
            }

            if (contaOrigem.Saldo < createDto.Valor)
            {
                throw new InvalidOperationException("Saldo insuficiente na conta de origem.");
            }

            var transferencia = new Transferencia
            {
                ContaOrigemId = contaOrigem.Id,
                ContaDestinoId = contaDestino.Id,
                Valor = createDto.Valor,
                DataTransferencia = DateTime.UtcNow
            };

            var transferenciaCriada = await _transferenciaRepository.CreateAsync(transferencia);

            contaOrigem.Saldo -= createDto.Valor;
            contaDestino.Saldo += createDto.Valor;

            await _contaRepository.UpdateAsync(contaOrigem);
            await _contaRepository.UpdateAsync(contaDestino);

            return MapToDto(transferenciaCriada);
        }

        private static TransferenciaDto MapToDto(Transferencia transferencia)
        {
            return new TransferenciaDto
            {
                Id = transferencia.Id,
                ContaOrigemNumero = transferencia.ContaOrigem?.Numero ?? "",
                ContaOrigemNome = transferencia.ContaOrigem?.Nome ?? "",
                ContaDestinoNumero = transferencia.ContaDestino?.Numero ?? "",
                ContaDestinoNome = transferencia.ContaDestino?.Nome ?? "",
                Valor = transferencia.Valor,
                DataTransferencia = transferencia.DataTransferencia
            };
        }

        private static string TirarFormatacaoDocumento(string documento)
        {
            return documento.Replace(".", "").Replace("-", "").Replace(" ", "");
        }
    }
}
