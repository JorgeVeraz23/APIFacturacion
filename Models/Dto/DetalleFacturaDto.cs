using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI1.Models.Dto
{
    public class DetalleFacturaDto
    {
        [Required]
        public int IdItem { get; set; }
        [Required]
        public int? IdFactura { get; set; }
        [Required]
        public int? IdUsuario { get; set; }
        public string? CodigoProducto { get; set; }

        public string NombreProducto { get; set; } = null!;

        public decimal Precio { get; set; }

        public int Cantidad { get; set; }

        public decimal Subtotal { get; set; }


    }
}
