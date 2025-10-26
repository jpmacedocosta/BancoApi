using BancoApi.Domain.Entities;
using BancoApi.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace BancoApi.Infrastructure.Data
{
    public class BancoDbContext : DbContext
    {
        public BancoDbContext(DbContextOptions<BancoDbContext> options) : base(options)
        {
        }

        public DbSet<Conta> Contas { get; set; } = null!;
        public DbSet<Transferencia> Transferencias { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new ContaConfiguration());
            modelBuilder.ApplyConfiguration(new TransferenciaConfiguration());
        }
    }
}
