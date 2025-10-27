using BancoApi.Application.DTOs;
using BancoApi.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace BancoApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TransferenciaController : ControllerBase
    {
        private readonly ITransferenciaService _transferenciaService;
        private readonly ILogger<TransferenciaController> _logger;

        public TransferenciaController(ITransferenciaService transferenciaService, ILogger<TransferenciaController> logger)
        {
            _transferenciaService = transferenciaService;
            _logger = logger;
        }

        /// <summary>
        /// Obtém transferências por documento da conta
        /// </summary>
        /// <param name="documento">Documento da conta</param>
        /// <returns>Lista de transferências da conta</returns>
        [HttpGet("conta/{documento}")]
        [ProducesResponseType(typeof(IEnumerable<TransferenciaDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<TransferenciaDto>>> GetTransferenciasPorConta(string documento)
        {
            try
            {
                var transferencias = await _transferenciaService.GetTransferenciasPorContaAsync(documento);
                return Ok(transferencias);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter transferências da conta {Documento}", documento);
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        /// <summary>
        /// Cria uma nova transferência
        /// </summary>
        /// <param name="createDto">Dados para criação da transferência</param>
        /// <returns>Transferência criada</returns>
        [HttpPost]
        [ProducesResponseType(typeof(TransferenciaDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TransferenciaDto>> CreateTransferencia([FromBody] CreateTransferenciaDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var transferencia = await _transferenciaService.CreateTransferenciaAsync(createDto);
                return StatusCode(201, transferencia);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar transferência");
                return StatusCode(500, "Erro interno do servidor");
            }
        }
    }
}
