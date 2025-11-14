// Criando este arquivo de contexto de banco de dados, pois é essencial
// e não foi fornecido. Assumo que AppDbContext e Usuario já existem.
using Microsoft.EntityFrameworkCore;
using SistemaChamadosApi.Models;

namespace SistemaChamadosApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Mapeamentos de tabelas existentes (assumidos)
        public DbSet<Usuario> Usuarios { get; set; }
        // public DbSet<Produto> Produtos { get; set; } // Assumindo que existe também

        // NOVO: Mapeamentos para o novo endpoint de chamado
        public DbSet<Servico> Servicos { get; set; }
        public DbSet<SolicitacaoServico> SolicitacoesServico { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Mapeamento da classe Usuario que já existe no seu projeto
            modelBuilder.Entity<Usuario>()
                .ToTable("USUARIO")
                .HasKey(u => u.IdUsuario); 
            
            // Aqui você pode adicionar outras configurações se necessário
            base.OnModelCreating(modelBuilder);
        }
    }
}