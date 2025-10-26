using System.ComponentModel.DataAnnotations;

namespace BancoApi.Domain.Entities
{
    public class Conta
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(6)]
        public required string Numero { get; set; }
        
        [Required]
        [StringLength(255)]
        public required string Nome { get; set; }
        
        [Required]
        [StringLength(14)]
        public required string Documento { get; set; }
        
        public decimal Saldo { get; set; }
        
        public DateTime DataCriacao { get; set; }
        
        public StatusConta Status { get; set; }

        public DateTime? DataAlteracao { get; set; }
    
        public string? UsuarioAlteracao { get; set; }
    }
    
    public enum StatusConta
    {
        Inativa = 0,
        Ativa = 1
    }
}
