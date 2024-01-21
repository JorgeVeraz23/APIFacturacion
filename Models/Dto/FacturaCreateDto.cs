using System.ComponentModel.DataAnnotations;

namespace FacturacionAPI1.Models.Dto
{
    public class FacturaCreateDto
    {

        [Required]
        public int IdFactura { get; set; }

        public int NumeroFactura { get; set; }

        public string RucCliente { get; set; } = null!;

        public string RazonSocialCliente { get; set; } = null!;

        public decimal Subtotal { get; set; }

        public decimal PorcentajeIgv { get; set; }

        public decimal Igv { get; set; }

        public decimal Total { get; set; }

        [Required]
        public int? IdUsuario { get; set; }

        
}
}
