using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FacturacionAPI1.Models;

public partial class Usuario
{
    [Key]
    public int IdUsuario { get; set; }

    [Column("Usuario")]
    [StringLength(50)]
    [Unicode(false)]
    public string Usuario1 { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string Contraseña { get; set; } = null!;

    public int? IntentosFallidos { get; set; }

    public bool? Bloqueado { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FechaCreacion { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FechaActualizacion { get; set; }

    [InverseProperty("IdUsuarioNavigation")]
    public virtual ICollection<DetalleFactura> DetalleFacturas { get; set; } = new List<DetalleFactura>();

    [InverseProperty("IdUsuarioNavigation")]
    public virtual ICollection<Factura> Facturas { get; set; } = new List<Factura>();

    [InverseProperty("IdUsuarioNavigation")]
    public virtual ICollection<FamiliaProducto> FamiliaProductos { get; set; } = new List<FamiliaProducto>();

    [InverseProperty("IdUsuarioNavigation")]
    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
