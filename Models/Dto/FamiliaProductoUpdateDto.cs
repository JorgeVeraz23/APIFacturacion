using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI1.Models.Dto
{
    public class FamiliaProductoUpdateDto
    {
        [Required]
        public int IdFamilia { get; set; }

        public string Codigo { get; set; } = null!;

        public string Nombre { get; set; } = null!;

        public bool? Activo { get; set; }

        public DateTime? FechaCreacion { get; set; }

        [Required]
        public int? IdUsuario { get; set; }
    }
}
