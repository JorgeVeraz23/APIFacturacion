using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI1.Models.Dto
{
    public class ProductoDto
    {
        [Required]
        public int IdProducto { get; set; }

        public string Codigo { get; set; } = null!;

        public string Nombre { get; set; } = null!;

        [Required]
        public int? IdFamilia { get; set; }

        public decimal Precio { get; set; }

        public int Stock { get; set; }

        public bool? Activo { get; set; }

        public DateTime? FechaCreacion { get; set; }

        [Required]
        public int? IdUsuario { get; set; }


    }
}
