using System.ComponentModel.DataAnnotations;

namespace BancoApi.Domain.Entities
{
    public class Conta
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(20)]
        public required string Numero { get; set; }
        
        [Required]
        [StringLength(100)]
        public required string Nome { get; set; }
        
        [Required]
        [StringLength(11)]
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
