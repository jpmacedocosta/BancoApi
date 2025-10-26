using BancoApi.Application.DTOs;
using BancoApi.Application.Services;
using BancoApi.Domain.Entities;
using BancoApi.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace BancoApi.Tests.Services
{
    public class TransferenciaServiceTests
    {
        private readonly Mock<ITransferenciaRepository> _mockTransferenciaRepository;
        private readonly Mock<IContaRepository> _mockContaRepository;
        private readonly TransferenciaService _transferenciaService;

        public TransferenciaServiceTests()
        {
            _mockTransferenciaRepository = new Mock<ITransferenciaRepository>();
            _mockContaRepository = new Mock<IContaRepository>();
            _transferenciaService = new TransferenciaService(
                _mockTransferenciaRepository.Object, 
                _mockContaRepository.Object);
        }

        [Fact]
        public async Task GetTransferenciaByIdAsync_DeveRetornarTransferencia_QuandoExistir()
        {
            var transferenciaId = 1;
            var transferencia = CriarTransferenciaParaTeste(transferenciaId);

            _mockTransferenciaRepository
                .Setup(r => r.GetByIdAsync(transferenciaId))
                .ReturnsAsync(transferencia);

            var resultado = await _transferenciaService.GetTransferenciaByIdAsync(transferenciaId);

            resultado.Should().NotBeNull();
            resultado!.Id.Should().Be(transferenciaId);
            resultado.Valor.Should().Be(500.00m);
            resultado.ContaOrigemNome.Should().Be("João Silva");
            resultado.ContaDestinoNome.Should().Be("Maria Santos");
        }

        [Fact]
        public async Task GetTransferenciaByIdAsync_DeveRetornarNull_QuandoNaoExistir()
        {
            var transferenciaId = 999;
            _mockTransferenciaRepository
                .Setup(r => r.GetByIdAsync(transferenciaId))
                .ReturnsAsync((Transferencia?)null);

            var resultado = await _transferenciaService.GetTransferenciaByIdAsync(transferenciaId);

            resultado.Should().BeNull();
        }

        [Fact]
        public async Task CreateTransferenciaAsync_DeveCriarTransferencia_QuandoDadosValidos()
        {
            var createDto = new CreateTransferenciaDto
            {
                DocumentoContaOrigem = "123.456.789-01",
                DocumentoContaDestino = "987.654.321-09",
                Valor = 250.00m
            };

            var contaOrigem = CriarContaParaTeste(1, "12345678901", "João Silva", 1000.00m);
            var contaDestino = CriarContaParaTeste(2, "98765432109", "Maria Santos", 500.00m);
            var transferenciaCreada = new Transferencia
            {
                Id = 1,
                ContaOrigemId = 1,
                ContaDestinoId = 2,
                Valor = 250.00m,
                DataTransferencia = DateTime.UtcNow,
                ContaOrigem = contaOrigem,
                ContaDestino = contaDestino
            };

            _mockContaRepository.Setup(r => r.GetByDocumentoAsync("12345678901")).ReturnsAsync(contaOrigem);
            _mockContaRepository.Setup(r => r.GetByDocumentoAsync("98765432109")).ReturnsAsync(contaDestino);
            _mockTransferenciaRepository.Setup(r => r.CreateAsync(It.IsAny<Transferencia>())).ReturnsAsync(transferenciaCreada);

            var resultado = await _transferenciaService.CreateTransferenciaAsync(createDto);

            resultado.Should().NotBeNull();
            resultado.Valor.Should().Be(250.00m);
            resultado.ContaOrigemNome.Should().Be("João Silva");
            resultado.ContaDestinoNome.Should().Be("Maria Santos");
        }

        [Fact]
        public async Task CreateTransferenciaAsync_DeveLancarExcecao_QuandoContaOrigemNaoExistir()
        {
            var createDto = new CreateTransferenciaDto
            {
                DocumentoContaOrigem = "999.999.999-99",
                DocumentoContaDestino = "987.654.321-09",
                Valor = 250.00m
            };

            _mockContaRepository.Setup(r => r.GetByDocumentoAsync("99999999999")).ReturnsAsync((Conta?)null);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _transferenciaService.CreateTransferenciaAsync(createDto));

            exception.Message.Should().Be("Conta de origem não encontrada.");
        }

        [Fact]
        public async Task CreateTransferenciaAsync_DeveLancarExcecao_QuandoContaDestinoNaoExistir()
        {
            var createDto = new CreateTransferenciaDto
            {
                DocumentoContaOrigem = "123.456.789-01",
                DocumentoContaDestino = "999.999.999-99",
                Valor = 250.00m
            };

            var contaOrigem = CriarContaParaTeste(1, "12345678901", "João Silva", 1000.00m);
            
            _mockContaRepository.Setup(r => r.GetByDocumentoAsync("12345678901")).ReturnsAsync(contaOrigem);
            _mockContaRepository.Setup(r => r.GetByDocumentoAsync("99999999999")).ReturnsAsync((Conta?)null);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _transferenciaService.CreateTransferenciaAsync(createDto));

            exception.Message.Should().Be("Conta de destino não encontrada.");
        }

        [Fact]
        public async Task CreateTransferenciaAsync_DeveLancarExcecao_QuandoContasForemIguais()
        {
            var createDto = new CreateTransferenciaDto
            {
                DocumentoContaOrigem = "123.456.789-01",
                DocumentoContaDestino = "123.456.789-01",
                Valor = 250.00m
            };

            var conta = CriarContaParaTeste(1, "12345678901", "João Silva", 1000.00m);
            
            _mockContaRepository.Setup(r => r.GetByDocumentoAsync("12345678901")).ReturnsAsync(conta);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _transferenciaService.CreateTransferenciaAsync(createDto));

            exception.Message.Should().Be("Conta de origem e destino não podem ser iguais.");
        }

        [Fact]
        public async Task CreateTransferenciaAsync_DeveLancarExcecao_QuandoContaOrigemInativa()
        {
            var createDto = new CreateTransferenciaDto
            {
                DocumentoContaOrigem = "123.456.789-01",
                DocumentoContaDestino = "987.654.321-09",
                Valor = 250.00m
            };

            var contaOrigem = CriarContaParaTeste(1, "12345678901", "João Silva", 1000.00m, StatusConta.Inativa);
            var contaDestino = CriarContaParaTeste(2, "98765432109", "Maria Santos", 500.00m);
            
            _mockContaRepository.Setup(r => r.GetByDocumentoAsync("12345678901")).ReturnsAsync(contaOrigem);
            _mockContaRepository.Setup(r => r.GetByDocumentoAsync("98765432109")).ReturnsAsync(contaDestino);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _transferenciaService.CreateTransferenciaAsync(createDto));

            exception.Message.Should().Be("Conta de origem deve estar ativa.");
        }

        [Fact]
        public async Task CreateTransferenciaAsync_DeveLancarExcecao_QuandoContaDestinoInativa()
        {
            var createDto = new CreateTransferenciaDto
            {
                DocumentoContaOrigem = "123.456.789-01",
                DocumentoContaDestino = "987.654.321-09",
                Valor = 250.00m
            };

            var contaOrigem = CriarContaParaTeste(1, "12345678901", "João Silva", 1000.00m);
            var contaDestino = CriarContaParaTeste(2, "98765432109", "Maria Santos", 500.00m, StatusConta.Inativa);
            
            _mockContaRepository.Setup(r => r.GetByDocumentoAsync("12345678901")).ReturnsAsync(contaOrigem);
            _mockContaRepository.Setup(r => r.GetByDocumentoAsync("98765432109")).ReturnsAsync(contaDestino);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _transferenciaService.CreateTransferenciaAsync(createDto));

            exception.Message.Should().Be("Conta de destino deve estar ativa.");
        }

        [Fact]
        public async Task CreateTransferenciaAsync_DeveLancarExcecao_QuandoSaldoInsuficiente()
        {
            var createDto = new CreateTransferenciaDto
            {
                DocumentoContaOrigem = "123.456.789-01",
                DocumentoContaDestino = "987.654.321-09",
                Valor = 1500.00m // Valor maior que o saldo
            };

            var contaOrigem = CriarContaParaTeste(1, "12345678901", "João Silva", 1000.00m);
            var contaDestino = CriarContaParaTeste(2, "98765432109", "Maria Santos", 500.00m);
            
            _mockContaRepository.Setup(r => r.GetByDocumentoAsync("12345678901")).ReturnsAsync(contaOrigem);
            _mockContaRepository.Setup(r => r.GetByDocumentoAsync("98765432109")).ReturnsAsync(contaDestino);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _transferenciaService.CreateTransferenciaAsync(createDto));

            exception.Message.Should().Be("Saldo insuficiente na conta de origem.");
        }

        [Fact]
        public async Task GetTransferenciasPorContaAsync_DeveRetornarTransferencias_QuandoContaExistir()
        {
            var documento = "12345678901";
            var transferencias = new List<Transferencia>
            {
                CriarTransferenciaParaTeste(1),
                CriarTransferenciaParaTeste(2)
            };

            _mockTransferenciaRepository
                .Setup(r => r.GetByDocumentoAsync(documento))
                .ReturnsAsync(transferencias);

            var resultado = await _transferenciaService.GetTransferenciasPorContaAsync(documento);

            resultado.Should().HaveCount(2);
            resultado.Should().OnlyContain(t => t.ContaOrigemNome == "João Silva" || t.ContaDestinoNome == "João Silva");
        }

        private static Conta CriarContaParaTeste(int id, string documento, string nome, decimal saldo, StatusConta status = StatusConta.Ativa)
        {
            return new Conta
            {
                Id = id,
                Numero = $"{10000 + id}-{id}",
                Nome = nome,
                Documento = documento,
                Saldo = saldo,
                DataCriacao = DateTime.UtcNow,
                Status = status
            };
        }

        private static Transferencia CriarTransferenciaParaTeste(int id)
        {
            return new Transferencia
            {
                Id = id,
                ContaOrigemId = 1,
                ContaDestinoId = 2,
                Valor = 500.00m,
                DataTransferencia = DateTime.UtcNow,
                ContaOrigem = new Conta
                {
                    Id = 1,
                    Numero = "12345-6",
                    Nome = "João Silva",
                    Documento = "12345678901",
                    Saldo = 1000.00m,
                    DataCriacao = DateTime.UtcNow,
                    Status = StatusConta.Ativa
                },
                ContaDestino = new Conta
                {
                    Id = 2,
                    Numero = "54321-0",
                    Nome = "Maria Santos",
                    Documento = "98765432109",
                    Saldo = 500.00m,
                    DataCriacao = DateTime.UtcNow,
                    Status = StatusConta.Ativa
                }
            };
        }
    }
}
