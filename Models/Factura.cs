using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace FacturacionAPI1.Models;

[Index("NumeroFactura", Name = "UQ__Facturas__CF12F9A6818C56EE", IsUnique = true)]
public partial class Factura
{
    [Key]
    public int IdFactura { get; set; }

    public int NumeroFactura { get; set; }

    [StringLength(15)]
    [Unicode(false)]
    public string RucCliente { get; set; } = null!;

    [StringLength(100)]
    [Unicode(false)]
    public string RazonSocialCliente { get; set; } = null!;

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Subtotal { get; set; }

    [Column("PorcentajeIGV", TypeName = "decimal(5, 2)")]
    public decimal PorcentajeIgv { get; set; }

    [Column("IGV", TypeName = "decimal(10, 2)")]
    public decimal Igv { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Total { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FechaCreacion { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FechaActualizacion { get; set; }

    public int? IdUsuario { get; set; }

    [InverseProperty("IdFacturaNavigation")]
    public virtual ICollection<DetalleFactura> DetalleFacturas { get; set; } = new List<DetalleFactura>();

    [ForeignKey("IdUsuario")]
    [InverseProperty("Facturas")]
    [JsonIgnore]
    public virtual Usuario? IdUsuarioNavigation { get; set; }
}
