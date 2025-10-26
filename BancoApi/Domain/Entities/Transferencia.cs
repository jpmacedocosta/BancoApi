using System.ComponentModel.DataAnnotations;

namespace BancoApi.Domain.Entities
{
    public class Transferencia
    {
        public int Id { get; set; }
        
        [Required]
        public int ContaOrigemId { get; set; }
        
        [Required]
        public int ContaDestinoId { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero")]
        public decimal Valor { get; set; }
        
        public DateTime DataTransferencia { get; set; }
        
        public StatusTransferencia Status { get; set; }
                
        public virtual Conta ContaOrigem { get; set; } = null!;
        public virtual Conta ContaDestino { get; set; } = null!;
    }
    
    public enum StatusTransferencia
    {
        Pendente = 0,
        Processada = 1,
        Rejeitada = 2,
        Cancelada = 3
    }
}
