// Nome do Arquivo: data/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using SistemaChamadosApi.Models; // Precisa importar a pasta models

namespace SistemaChamadosApi.Data // Namespace correto: Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Mapeia suas classes para as tabelas do PIM
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Produto> Produtos { get; set; }
        public DbSet<Servico> Servicos { get; set; }
        public DbSet<SolicitacaoServico> SolicitacoesServico { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Garante que o EF Core use os nomes de tabela exatos do PIM
            modelBuilder.Entity<Usuario>().ToTable("USUARIO");
            modelBuilder.Entity<Produto>().ToTable("PRODUTO");
            modelBuilder.Entity<Servico>().ToTable("SERVICO");
            modelBuilder.Entity<SolicitacaoServico>().ToTable("SOLICITACAO_SERVICO");
        }
    }
}