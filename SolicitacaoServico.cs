// Nome do Arquivo: models/SolicitacaoServico.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaChamadosApi.Models // Namespace correto: Models
{
    [Table("SOLICITACAO_SERVICO")]
    public class SolicitacaoServico
    {
        [Key]
        [Column("ID_SOLICITACAO")]
        public int IdSolicitacao { get; set; }

        [Column("ID_SERVICO")]
        public int IdServico { get; set; }

        [Column("ID_USUARIO")]
        public int IdUsuario { get; set; }

        [Column("DATA_SOLICITACAO")]
        public DateTime DataSolicitacao { get; set; } = DateTime.Now;

        [Required]
        [Column("DESCRICAO")]
        public string Descricao { get; set; } = string.Empty;

        [Required]
        [Column("STATUS")]
        public string Status { get; set; } = string.Empty;

        [Required]
        [Column("PRIORIDADE")]
        public string Prioridade { get; set; } = string.Empty;

        [Column("ANALISE_IA")]
        public string? AnaliseIa { get; set; }

        // Relacionamentos para o EF Core
        [ForeignKey("IdUsuario")]
        public virtual Usuario? Usuario { get; set; }

        [ForeignKey("IdServico")]
        public virtual Servico? Servico { get; set; }
    }
}