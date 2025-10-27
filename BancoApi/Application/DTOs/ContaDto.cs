using System.ComponentModel.DataAnnotations;
using BancoApi.Domain.Entities;

namespace BancoApi.Application.DTOs
{
    public class ContaDto
    {
        public int Id { get; set; }
        public string Numero { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Documento { get; set; } = string.Empty;
        public decimal Saldo { get; set; }
        public DateTime DataCriacao { get; set; }
        public StatusConta Status { get; set; }
        public string StatusDescricao => Status.ToString();
    }

    public class CreateContaDto
    {
        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(255, ErrorMessage = "O nome deve ter no máximo 255 caracteres")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O documento é obrigatório")]
        [StringLength(14, ErrorMessage = "O documento deve ter no máximo 14 caracteres")]
        public string Documento { get; set; } = string.Empty;
    }
}
