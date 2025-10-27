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
        /// Obtém uma conta por ID
        /// </summary>
        /// <param name="id">ID da conta</param>
        /// <returns>Dados da conta</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ContaDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ContaDto>> GetConta(int id)
        {
            try
            {
                var conta = await _contaService.GetContaByIdAsync(id);
                if (conta == null)
                {
                    return NotFound($"Conta com ID {id} não encontrada");
                }

                return Ok(conta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter conta com ID {Id}", id);
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        /// <summary>
        /// Obtém contas por nome ou documento com paginação
        /// </summary>
        /// <param name="termo">Nome ou Documento do usuário da conta</param>
        /// <param name="page">Número da página (padrão: 1)</param>
        /// <param name="pageSize">Itens por página (padrão: 10, máximo: 100)</param>
        /// <returns>Lista paginada de contas encontradas</returns>
        [HttpGet("termo={termo}")]
        [ProducesResponseType(typeof(PagedResult<ContaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PagedResult<ContaDto>>> GetContaByNomeOrDocumento(
            string termo,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(termo))
                {
                    return BadRequest("O termo de busca é obrigatório");
                }

                if (page < 1)
                {
                    return BadRequest("A página deve ser maior que 0");
                }

                if (pageSize < 1 || pageSize > 100)
                {
                    return BadRequest("O tamanho da página deve estar entre 1 e 100");
                }

                var resultado = await _contaService.GetContaByNomeOrDocumentoPaginatedAsync(termo, page, pageSize);
                
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
                return CreatedAtAction(nameof(GetConta), new { id = conta.Id }, conta);
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
