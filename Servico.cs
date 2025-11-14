// Nome do Arquivo: models/Servico.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaChamadosApi.Models // Namespace correto: Models
{
    [Table("SERVICO")]
    public class Servico
    {
        [Key]
        [Column("ID_SERVICO")]
        public int IdServico { get; set; }

        [Column("DATA_SERVICO")]
        public DateTime DataServico { get; set; } = DateTime.Now;

        [Required]
        [Column("DESCRICAO_SERVICO")]
        public string DescricaoServico { get; set; } = string.Empty;

        [Column("ID_USUARIO")]
        public int IdUsuario { get; set; }

        [ForeignKey("IdUsuario")]
        public virtual Usuario? Usuario { get; set; }
    }
}