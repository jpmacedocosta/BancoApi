using BancoApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BancoApi.Infrastructure.Data.Configurations
{
    public class ContaConfiguration : IEntityTypeConfiguration<Conta>
    {
        public void Configure(EntityTypeBuilder<Conta> builder)
        {
            builder.ToTable("contas");
            
            builder.HasKey(e => e.Id);
            
            builder.Property(e => e.Id)
                .HasColumnName("id");
            
            builder.Property(e => e.Numero)
                .IsRequired()
                .HasMaxLength(6)
                .HasColumnName("numero");

            builder.Property(e => e.Nome)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("nome");

            builder.Property(e => e.Documento)
                .IsRequired()
                .HasMaxLength(14)
                .HasColumnName("documento");

            builder.Property(e => e.Saldo)
                .HasColumnType("decimal(18,2)")
                .HasColumnName("saldo");

            builder.Property(e => e.DataCriacao)
                .IsRequired()
                .HasColumnName("data_criacao");

            builder.Property(e => e.Status)
                .IsRequired()
                .HasConversion<int>()
                .HasColumnName("status");

            builder.Property(e => e.DataAlteracao)
                .IsRequired(false)
                .HasColumnName("data_alteracao");

            builder.Property(e => e.UsuarioAlteracao)
                .IsRequired(false)
                .HasColumnName("usuario_alteracao");

            builder.HasIndex(e => e.Numero)
                .IsUnique()
                .HasDatabaseName("ix_contas_numero");

            builder.HasIndex(e => e.Documento)
                .HasDatabaseName("ix_contas_documento");
        }
    }
}
