using BancoApi.Application.DTOs;
using BancoApi.Application.Services;
using BancoApi.Controllers;
using BancoApi.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace BancoApi.Tests.Controllers
{
    public class TransferenciaControllerTests
    {
        private readonly Mock<ITransferenciaService> _mockTransferenciaService;
        private readonly Mock<ILogger<TransferenciaController>> _mockLogger;
        private readonly TransferenciaController _controller;

        public TransferenciaControllerTests()
        {
            _mockTransferenciaService = new Mock<ITransferenciaService>();
            _mockLogger = new Mock<ILogger<TransferenciaController>>();
            _controller = new TransferenciaController(_mockTransferenciaService.Object, _mockLogger.Object);
        }



        [Fact]
        public async Task GetTransferenciasPorConta_DeveRetornarOk_ComListaDeTransferencias()
        {
            var documento = "12345678901";
            var transferencias = new List<TransferenciaDto>
            {
                CriarTransferenciaDtoParaTeste(1),
                CriarTransferenciaDtoParaTeste(2)
            };

            _mockTransferenciaService
                .Setup(s => s.GetTransferenciasPorContaAsync(documento))
                .ReturnsAsync(transferencias);

            var resultado = await _controller.GetTransferenciasPorConta(documento);

            var actionResult = resultado.Result as OkObjectResult;
            actionResult.Should().NotBeNull();
            actionResult!.StatusCode.Should().Be(200);
            
            var transferenciasRetornadas = actionResult.Value as IEnumerable<TransferenciaDto>;
            transferenciasRetornadas.Should().NotBeNull();
            transferenciasRetornadas!.Should().HaveCount(2);
        }

        [Fact]
        public async Task CreateTransferencia_DeveRetornarCreated_QuandoDadosValidos()
        {
            var createDto = new CreateTransferenciaDto
            {
                DocumentoContaOrigem = "123.456.789-01",
                DocumentoContaDestino = "987.654.321-09",
                Valor = 500.00m
            };

            var transferenciaCriada = CriarTransferenciaDtoParaTeste(1);

            _mockTransferenciaService
                .Setup(s => s.CreateTransferenciaAsync(createDto))
                .ReturnsAsync(transferenciaCriada);

            var resultado = await _controller.CreateTransferencia(createDto);

            var actionResult = resultado.Result as ObjectResult;
            actionResult.Should().NotBeNull();
            actionResult!.StatusCode.Should().Be(201);
            
            var transferenciaRetornada = actionResult.Value as TransferenciaDto;
            transferenciaRetornada.Should().NotBeNull();
            transferenciaRetornada!.Valor.Should().Be(500.00m);
        }

        [Fact]
        public async Task CreateTransferencia_DeveRetornarBadRequest_QuandoModelStateInvalido()
        {
            var createDto = new CreateTransferenciaDto
            {
                DocumentoContaOrigem = "", // Documento vazio para forçar erro
                DocumentoContaDestino = "987.654.321-09",
                Valor = 500.00m
            };

            _controller.ModelState.AddModelError("DocumentoContaOrigem", "Documento da conta de origem é obrigatório");

            var resultado = await _controller.CreateTransferencia(createDto);

            var actionResult = resultado.Result as BadRequestObjectResult;
            actionResult.Should().NotBeNull();
            actionResult!.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task CreateTransferencia_DeveRetornarBadRequest_QuandoContaNaoExistir()
        {
            var createDto = new CreateTransferenciaDto
            {
                DocumentoContaOrigem = "999.999.999-99",
                DocumentoContaDestino = "987.654.321-09",
                Valor = 500.00m
            };

            _mockTransferenciaService
                .Setup(s => s.CreateTransferenciaAsync(createDto))
                .ThrowsAsync(new InvalidOperationException("Conta de origem não encontrada."));

            var resultado = await _controller.CreateTransferencia(createDto);

            var actionResult = resultado.Result as BadRequestObjectResult;
            actionResult.Should().NotBeNull();
            actionResult!.StatusCode.Should().Be(400);
            actionResult.Value.Should().Be("Conta de origem não encontrada.");
        }

        [Fact]
        public async Task CreateTransferencia_DeveRetornarBadRequest_QuandoSaldoInsuficiente()
        {
            var createDto = new CreateTransferenciaDto
            {
                DocumentoContaOrigem = "123.456.789-01",
                DocumentoContaDestino = "987.654.321-09",
                Valor = 10000.00m // Valor muito alto
            };

            _mockTransferenciaService
                .Setup(s => s.CreateTransferenciaAsync(createDto))
                .ThrowsAsync(new InvalidOperationException("Saldo insuficiente na conta de origem."));

            var resultado = await _controller.CreateTransferencia(createDto);

            var actionResult = resultado.Result as BadRequestObjectResult;
            actionResult.Should().NotBeNull();
            actionResult!.StatusCode.Should().Be(400);
            actionResult.Value.Should().Be("Saldo insuficiente na conta de origem.");
        }

        [Theory]
        [InlineData(0.00)]
        [InlineData(-100.00)]
        public async Task CreateTransferencia_DeveRetornarBadRequest_QuandoValorInvalido(decimal valorInvalido)
        {
            var createDto = new CreateTransferenciaDto
            {
                DocumentoContaOrigem = "123.456.789-01",
                DocumentoContaDestino = "987.654.321-09",
                Valor = valorInvalido
            };

            _controller.ModelState.AddModelError("Valor", "O valor deve ser maior que zero");

            var resultado = await _controller.CreateTransferencia(createDto);

            var actionResult = resultado.Result as BadRequestObjectResult;
            actionResult.Should().NotBeNull();
            actionResult!.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task CreateTransferencia_DeveRetornarInternalServerError_QuandoOcorrerExcecaoGenerica()
        {
            var createDto = new CreateTransferenciaDto
            {
                DocumentoContaOrigem = "123.456.789-01",
                DocumentoContaDestino = "987.654.321-09",
                Valor = 500.00m
            };

            _mockTransferenciaService
                .Setup(s => s.CreateTransferenciaAsync(createDto))
                .ThrowsAsync(new Exception("Erro inesperado"));

            var resultado = await _controller.CreateTransferencia(createDto);

            var actionResult = resultado.Result as ObjectResult;
            actionResult.Should().NotBeNull();
            actionResult!.StatusCode.Should().Be(500);
            actionResult.Value.Should().Be("Erro interno do servidor");
        }

        [Fact]
        public async Task GetTransferenciasPorConta_DeveRetornarListaVazia_QuandoNaoHouverTransferencias()
        {
            var documento = "12345678901";
            _mockTransferenciaService
                .Setup(s => s.GetTransferenciasPorContaAsync(documento))
                .ReturnsAsync(new List<TransferenciaDto>());

            var resultado = await _controller.GetTransferenciasPorConta(documento);

            var actionResult = resultado.Result as OkObjectResult;
            actionResult.Should().NotBeNull();
            actionResult!.StatusCode.Should().Be(200);
            
            var transferenciasRetornadas = actionResult.Value as IEnumerable<TransferenciaDto>;
            transferenciasRetornadas.Should().NotBeNull();
            transferenciasRetornadas!.Should().BeEmpty();
        }

        private static TransferenciaDto CriarTransferenciaDtoParaTeste(int id)
        {
            return new TransferenciaDto
            {
                Id = id,
                ContaOrigemNumero = "12345-6",
                ContaOrigemNome = "João Silva",
                ContaDestinoNumero = "54321-0",
                ContaDestinoNome = "Maria Santos",
                Valor = 500.00m,
                DataTransferencia = DateTime.UtcNow
            };
        }
    }
}
