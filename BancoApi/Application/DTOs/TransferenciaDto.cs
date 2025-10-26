using System.ComponentModel.DataAnnotations;

namespace BancoApi.Application.DTOs
{
    public class TransferenciaDto
    {
        public int Id { get; set; }
        public string ContaOrigemNumero { get; set; } = string.Empty;
        public string ContaOrigemNome { get; set; } = string.Empty;
        public string ContaDestinoNumero { get; set; } = string.Empty;
        public string ContaDestinoNome { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public DateTime DataTransferencia { get; set; }
    }

    public class CreateTransferenciaDto
    {
        [Required(ErrorMessage = "Documento da conta de origem é obrigatório")]
        public required string DocumentoContaOrigem { get; set; }

        [Required(ErrorMessage = "Documento da conta de destino é obrigatório")]
        public required string DocumentoContaDestino { get; set; }

        [Required(ErrorMessage = "Valor é obrigatório")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero")]
        public decimal Valor { get; set; }
    }
}
