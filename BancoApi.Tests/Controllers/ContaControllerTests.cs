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
    public class ContaControllerTests
    {
        private readonly Mock<IContaService> _mockContaService;
        private readonly Mock<ILogger<ContaController>> _mockLogger;
        private readonly ContaController _controller;

        public ContaControllerTests()
        {
            _mockContaService = new Mock<IContaService>();
            _mockLogger = new Mock<ILogger<ContaController>>();
            _controller = new ContaController(_mockContaService.Object, _mockLogger.Object);
        }



        [Fact]
        public async Task GetContaByNomeOrDocumento_DeveRetornarOk_QuandoContasExistirem()
        {
            var termo = "joão";
            var pagination = new PaginationRequest { Page = 1, PageSize = 10 };
            var contasDto = new List<ContaDto>
            {
                new ContaDto
                {
                    Id = 1,
                    Numero = "12345-6",
                    Nome = "João Silva",
                    Documento = "12345678901",
                    Saldo = 1000.00m,
                    DataCriacao = DateTime.UtcNow,
                    Status = StatusConta.Ativa
                },
                new ContaDto
                {
                    Id = 2,
                    Numero = "54321-0",
                    Nome = "João Santos",
                    Documento = "98765432109",
                    Saldo = 500.00m,
                    DataCriacao = DateTime.UtcNow,
                    Status = StatusConta.Ativa
                }
            };

            var pagedResult = new PagedResult<ContaDto>
            {
                Items = contasDto,
                TotalItems = 2,
                Page = 1,
                PageSize = 10,
                TotalPages = 1
            };

            _mockContaService
                .Setup(s => s.GetContaByNomeOrDocumentoPaginatedAsync(termo, 1, 10))
                .ReturnsAsync(pagedResult);

            var resultado = await _controller.GetContaByNomeOrDocumento(termo, pagination);

            var actionResult = resultado.Result as OkObjectResult;
            actionResult.Should().NotBeNull();
            actionResult!.StatusCode.Should().Be(200);
            
            var resultadoPaginado = actionResult.Value as PagedResult<ContaDto>;
            resultadoPaginado.Should().NotBeNull();
            resultadoPaginado!.Items.Should().HaveCount(2);
            resultadoPaginado.Items.Should().OnlyContain(c => c.Nome.Contains("João"));
            resultadoPaginado.TotalItems.Should().Be(2);
            resultadoPaginado.Page.Should().Be(1);
        }

        [Fact]
        public async Task GetContaByNomeOrDocumento_DeveRetornarNotFound_QuandoNenhumaContaExistir()
        {
            var termo = "inexistente";
            var pagination = new PaginationRequest { Page = 1, PageSize = 10 };
            var pagedResult = new PagedResult<ContaDto>
            {
                Items = new List<ContaDto>(),
                TotalItems = 0,
                Page = 1,
                PageSize = 10,
                TotalPages = 0
            };

            _mockContaService
                .Setup(s => s.GetContaByNomeOrDocumentoPaginatedAsync(termo, 1, 10))
                .ReturnsAsync(pagedResult);

            var resultado = await _controller.GetContaByNomeOrDocumento(termo, pagination);

            var actionResult = resultado.Result as NotFoundObjectResult;
            actionResult.Should().NotBeNull();
            actionResult!.StatusCode.Should().Be(404);
            actionResult.Value.Should().Be($"Nenhuma conta encontrada com o termo: {termo}");
        }

        [Fact]
        public async Task CreateConta_DeveRetornarCreated_QuandoDadosValidos()
        {
            var createDto = new CreateContaDto
            {
                Nome = "Maria Santos",
                Documento = "987.654.321-09"
            };

            var contaCriada = new ContaDto
            {
                Id = 1,
                Numero = "54321-0",
                Nome = "Maria Santos",
                Documento = "98765432109",
                Saldo = 1000.00m,
                DataCriacao = DateTime.UtcNow,
                Status = StatusConta.Ativa
            };

            _mockContaService
                .Setup(s => s.CreateContaAsync(createDto))
                .ReturnsAsync(contaCriada);

            var resultado = await _controller.CreateConta(createDto);

            var actionResult = resultado.Result as ObjectResult;
            actionResult.Should().NotBeNull();
            actionResult!.StatusCode.Should().Be(201);
            
            var contaRetornada = actionResult.Value as ContaDto;
            contaRetornada.Should().NotBeNull();
            contaRetornada!.Nome.Should().Be("Maria Santos");
            contaRetornada.Documento.Should().Be("98765432109");
        }

        [Fact]
        public async Task CreateConta_DeveRetornarBadRequest_QuandoModelStateInvalido()
        {
            var createDto = new CreateContaDto
            {
                Nome = "", // Nome vazio para forçar erro de validação
                Documento = "987.654.321-09"
            };

            _controller.ModelState.AddModelError("Nome", "Nome é obrigatório");

            var resultado = await _controller.CreateConta(createDto);

            var actionResult = resultado.Result as BadRequestObjectResult;
            actionResult.Should().NotBeNull();
            actionResult!.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task CreateConta_DeveRetornarBadRequest_QuandoContaJaExistir()
        {
            var createDto = new CreateContaDto
            {
                Nome = "João Silva",
                Documento = "123.456.789-01"
            };

            _mockContaService
                .Setup(s => s.CreateContaAsync(createDto))
                .ThrowsAsync(new InvalidOperationException("Já existe uma conta com o mesmo nome ou documento."));

            var resultado = await _controller.CreateConta(createDto);

            var actionResult = resultado.Result as BadRequestObjectResult;
            actionResult.Should().NotBeNull();
            actionResult!.StatusCode.Should().Be(400);
            actionResult.Value.Should().Be("Já existe uma conta com o mesmo nome ou documento.");
        }

        [Fact]
        public async Task InativarConta_DeveRetornarOk_QuandoContaForInativadaComSucesso()
        {
            var documento = "12345678901";
            var contaInativada = new ContaDto
            {
                Id = 1,
                Numero = "12345-6",
                Nome = "João Silva",
                Documento = documento,
                Saldo = 1000.00m,
                DataCriacao = DateTime.UtcNow,
                Status = StatusConta.Inativa
            };

            _mockContaService
                .Setup(s => s.InativarContaAsync(documento))
                .ReturnsAsync(contaInativada);

            var resultado = await _controller.InativarConta(documento);

            var actionResult = resultado.Result as OkObjectResult;
            actionResult.Should().NotBeNull();
            actionResult!.StatusCode.Should().Be(200);
            
            var contaRetornada = actionResult.Value as ContaDto;
            contaRetornada.Should().NotBeNull();
            contaRetornada!.Status.Should().Be(StatusConta.Inativa);
        }

        [Fact]
        public async Task InativarConta_DeveRetornarNotFound_QuandoContaNaoExistir()
        {
            var documento = "99999999999";
            _mockContaService
                .Setup(s => s.InativarContaAsync(documento))
                .ReturnsAsync((ContaDto?)null);

            var resultado = await _controller.InativarConta(documento);

            var actionResult = resultado.Result as NotFoundObjectResult;
            actionResult.Should().NotBeNull();
            actionResult!.StatusCode.Should().Be(404);
            actionResult.Value.Should().Be($"Conta com o documento {documento} não encontrada");
        }

        [Fact]
        public async Task InativarConta_DeveChamarServicoComIpCorreto()
        {
            var documento = "12345678901";
            var contaInativada = new ContaDto
            {
                Id = 1,
                Numero = "12345-6",
                Nome = "João Silva",
                Documento = documento,
                Saldo = 1000.00m,
                DataCriacao = DateTime.UtcNow,
                Status = StatusConta.Inativa
            };

            _mockContaService
                .Setup(s => s.InativarContaAsync(documento))
                .ReturnsAsync(contaInativada);

            await _controller.InativarConta(documento);

            _mockContaService.Verify(s => s.InativarContaAsync(documento), Times.Once);
        }
    }
}
