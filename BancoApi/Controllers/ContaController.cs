using BancoApi.Application.DTOs;
using BancoApi.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace BancoApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ContaController : ControllerBase
    {
        private readonly IContaService _contaService;
        private readonly ILogger<ContaController> _logger;

        public ContaController(IContaService contaService, ILogger<ContaController> logger)
        {
            _contaService = contaService;
            _logger = logger;
        }

        /// <summary>
        /// Obtém contas por nome ou documento com paginação
        /// </summary>
        /// <param name="termo">Nome ou Documento do usuário da conta</param>
        /// <param name="paginationRequest">Parâmetros de paginação</param>
        /// <returns>Lista paginada de contas encontradas</returns>
        [HttpGet("termo={termo}")]
        [ProducesResponseType(typeof(PagedResult<ContaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PagedResult<ContaDto>>> GetContaByNomeOrDocumento(
            string termo,
            [FromQuery] PaginationRequest pagination)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(termo))
                {
                    return BadRequest("O termo de busca é obrigatório");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var resultado = await _contaService.GetContaByNomeOrDocumentoPaginatedAsync(termo, pagination.Page, pagination.PageSize);

                if (!resultado.Items.Any())
                {
                    return NotFound($"Nenhuma conta encontrada com o termo: {termo}");
                }

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter contas com termo {termo}", termo);
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        /// <summary>
        /// Cria uma nova conta
        /// </summary>
        /// <param name="createContaDto">Dados para criação da conta</param>
        /// <returns>Conta criada</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ContaDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ContaDto>> CreateConta([FromBody] CreateContaDto createContaDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var conta = await _contaService.CreateContaAsync(createContaDto);
                return StatusCode(201, conta);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar conta");
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        /// <summary>
        /// Inativa uma conta existente ativa
        /// </summary>
        /// <param name="documento">Documento da conta</param>
        /// <returns>Conta atualizada</returns>
        [HttpPatch("{documento}/inativar")]
        [ProducesResponseType(typeof(ContaDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ContaDto>> InativarConta(string documento)
        {
            try
            {
                var conta = await _contaService.InativarContaAsync(documento);
                if (conta == null)
                {
                    return NotFound($"Conta com o documento {documento} não encontrada");
                }

                return Ok(conta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar conta com documento {Documento}", documento);
                return StatusCode(500, "Erro interno do servidor");
            }
        }
    }
}
