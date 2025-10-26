using BancoApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BancoApi.Infrastructure.Data.Configurations
{
    public class TransferenciaConfiguration : IEntityTypeConfiguration<Transferencia>
    {
        public void Configure(EntityTypeBuilder<Transferencia> builder)
        {
            builder.ToTable("transferencias");
            
            builder.HasKey(e => e.Id);
            
            builder.Property(e => e.Id)
                .HasColumnName("id");
            
            builder.Property(e => e.ContaOrigemId)
                .IsRequired()
                .HasColumnName("conta_origem_id");
            
            builder.Property(e => e.ContaDestinoId)
                .IsRequired()
                .HasColumnName("conta_destino_id");
            
            builder.Property(e => e.Valor)
                .HasColumnType("decimal(18,2)")
                .IsRequired()
                .HasColumnName("valor");

            builder.Property(e => e.DataTransferencia)
                .IsRequired()
                .HasColumnName("data_transferencia");

            builder.HasOne(t => t.ContaOrigem)
                .WithMany()
                .HasForeignKey(t => t.ContaOrigemId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_transferencia_conta_origem");

            builder.HasOne(t => t.ContaDestino)
                .WithMany()
                .HasForeignKey(t => t.ContaDestinoId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_transferencia_conta_destino");

            builder.HasIndex(e => e.ContaOrigemId)
                .HasDatabaseName("ix_transferencia_conta_origem_id");
            
            builder.HasIndex(e => e.ContaDestinoId)
                .HasDatabaseName("ix_transferencia_conta_destino_id");
        }
    }
}
