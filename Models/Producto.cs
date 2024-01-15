using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace FacturacionAPI1.Models;

[Index("Codigo", Name = "UQ__Producto__06370DACC34B4B39", IsUnique = true)]
public partial class Producto
{
    [Key]
    public int IdProducto { get; set; }

    [StringLength(15)]
    [Unicode(false)]
    public string Codigo { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string Nombre { get; set; } = null!;

    public int? IdFamilia { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Precio { get; set; }

    public int Stock { get; set; }

    public bool? Activo { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FechaCreacion { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FechaActualizacion { get; set; }

    public int? IdUsuario { get; set; }

    public virtual ICollection<DetalleFactura> DetalleFacturas { get; set; } = new List<DetalleFactura>();

    [ForeignKey("IdFamilia")]
    [InverseProperty("Productos")]
    public virtual FamiliaProducto? IdFamiliaNavigation { get; set; }

    [ForeignKey("IdUsuario")]
    [InverseProperty("Productos")]
    [JsonIgnore]
    public virtual Usuario? IdUsuarioNavigation { get; set; }
}
