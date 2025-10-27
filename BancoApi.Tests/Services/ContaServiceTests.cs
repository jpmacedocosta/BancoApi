using BancoApi.Application.DTOs;
using BancoApi.Application.Services;
using BancoApi.Domain.Entities;
using BancoApi.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace BancoApi.Tests.Services
{
    public class ContaServiceTests
    {
        private readonly Mock<IContaRepository> _mockContaRepository;
        private readonly ContaService _contaService;

        public ContaServiceTests()
        {
            _mockContaRepository = new Mock<IContaRepository>();
            _contaService = new ContaService(_mockContaRepository.Object);
        }



        [Fact]
        public async Task CreateContaAsync_DeveCriarConta_QuandoDadosValidos()
        {
            var createDto = new CreateContaDto
            {
                Nome = "Maria Santos",
                Documento = "987.654.321-09"
            };

            var contaCriada = new Conta
            {
                Id = 1,
                Numero = "54321-0",
                Nome = "Maria Santos",
                Documento = "98765432109",
                Saldo = 1000.00m,
                DataCriacao = DateTime.UtcNow,
                Status = StatusConta.Ativa
            };

            _mockContaRepository
                .Setup(r => r.GetByDocumentoAsync("98765432109"))
                .ReturnsAsync((Conta?)null);

            _mockContaRepository
                .Setup(r => r.ExistsByNumeroAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            _mockContaRepository
                .Setup(r => r.CreateAsync(It.IsAny<Conta>()))
                .ReturnsAsync(contaCriada);

            var resultado = await _contaService.CreateContaAsync(createDto);

            resultado.Should().NotBeNull();
            resultado.Nome.Should().Be("Maria Santos");
            resultado.Documento.Should().Be("98765432109");
            resultado.Saldo.Should().Be(1000.00m);
            resultado.Status.Should().Be(StatusConta.Ativa);
        }

        [Fact]
        public async Task CreateContaAsync_DeveLancarExcecao_QuandoContaJaExistir()
        {
            var createDto = new CreateContaDto
            {
                Nome = "João Silva",
                Documento = "123.456.789-01"
            };

            var contaExistente = new Conta
            {
                Id = 1,
                Numero = "12345-6",
                Nome = "João Silva",
                Documento = "12345678901",
                Saldo = 1000.00m,
                DataCriacao = DateTime.UtcNow,
                Status = StatusConta.Ativa
            };

            _mockContaRepository
                .Setup(r => r.GetByDocumentoAsync("12345678901"))
                .ReturnsAsync(contaExistente);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _contaService.CreateContaAsync(createDto));

            exception.Message.Should().Be("Já existe uma conta com o mesmo número de documento.");
        }

        [Fact]
        public async Task InativarContaAsync_DeveInativarConta_QuandoContaAtivaExistir()
        {
            var documento = "12345678901";
            var conta = new Conta
            {
                Id = 1,
                Numero = "12345-6",
                Nome = "João Silva",
                Documento = documento,
                Saldo = 1000.00m,
                DataCriacao = DateTime.UtcNow,
                Status = StatusConta.Ativa
            };

            var contaInativada = new Conta
            {
                Id = 1,
                Numero = "12345-6",
                Nome = "João Silva",
                Documento = documento,
                Saldo = 1000.00m,
                DataCriacao = DateTime.UtcNow,
                Status = StatusConta.Inativa,
                DataAlteracao = DateTime.UtcNow,
                UsuarioAlteracao = "Sistema"
            };

            _mockContaRepository
                .Setup(r => r.GetByDocumentoAsync(documento))
                .ReturnsAsync(conta);

            _mockContaRepository
                .Setup(r => r.UpdateAsync(It.IsAny<Conta>()))
                .ReturnsAsync(contaInativada);

            var resultado = await _contaService.InativarContaAsync(documento);

            resultado.Should().NotBeNull();
            resultado!.Status.Should().Be(StatusConta.Inativa);
            
            _mockContaRepository.Verify(r => r.UpdateAsync(It.Is<Conta>(c => 
                c.Status == StatusConta.Inativa && 
                c.UsuarioAlteracao == "Sistema")), Times.Once);
        }

        [Fact]
        public async Task InativarContaAsync_DeveRetornarNull_QuandoContaNaoExistir()
        {
            var documento = "99999999999";
            _mockContaRepository
                .Setup(r => r.GetByDocumentoAsync(documento))
                .ReturnsAsync((Conta?)null);

            var resultado = await _contaService.InativarContaAsync(documento);

            resultado.Should().BeNull();
        }

        [Fact]
        public async Task InativarContaAsync_DeveLancarExcecao_QuandoContaJaInativa()
        {
            var documento = "12345678901";
            var conta = new Conta
            {
                Id = 1,
                Numero = "12345-6",
                Nome = "João Silva",
                Documento = documento,
                Saldo = 1000.00m,
                DataCriacao = DateTime.UtcNow,
                Status = StatusConta.Inativa
            };

            _mockContaRepository
                .Setup(r => r.GetByDocumentoAsync(documento))
                .ReturnsAsync(conta);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _contaService.InativarContaAsync(documento));

            exception.Message.Should().Be("A conta informada já está inativa.");
        }

        [Theory]
        [InlineData("123.456.789-01", "12345678901")]
        [InlineData("123 456 789 01", "12345678901")]
        [InlineData("123-456-789-01", "12345678901")]
        [InlineData("12345678901", "12345678901")]
        public async Task GetContaByDocumentoAsync_DeveNormalizarDocumento_ParaDiferentesFormatos(
            string documentoEntrada, string documentoEsperado)
        {
            var conta = new Conta
            {
                Id = 1,
                Numero = "12345-6",
                Nome = "João Silva",
                Documento = documentoEsperado,
                Saldo = 1000.00m,
                DataCriacao = DateTime.UtcNow,
                Status = StatusConta.Ativa
            };

            _mockContaRepository
                .Setup(r => r.GetByDocumentoAsync(documentoEsperado))
                .ReturnsAsync(conta);

            var resultado = await _contaService.GetContaByDocumentoAsync(documentoEntrada);

            resultado.Should().NotBeNull();
            resultado!.Documento.Should().Be(documentoEsperado);
            
            _mockContaRepository.Verify(r => r.GetByDocumentoAsync(documentoEsperado), Times.Once);
        }
    }
}
