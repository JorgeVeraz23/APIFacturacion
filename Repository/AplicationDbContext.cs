using System;
using System.Windows;
using System.Collections.Generic;
using FacturacionAPI1.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.SqlServer;

namespace FacturacionAPI1.Repository;

public partial class AplicationDbContext : DbContext
{
    public AplicationDbContext()
    {
    }

    public AplicationDbContext(DbContextOptions<AplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<DetalleFactura> DetalleFacturas { get; set; }

    public virtual DbSet<Factura> Facturas { get; set; }

    public virtual DbSet<FamiliaProducto> FamiliaProductos { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=DESKTOP-K3LB2V2;Initial Catalog=SistemaFacturacion;Integrated Security=True; TrustServerCertificate=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DetalleFactura>(entity =>
        {
            entity.HasKey(e => e.IdItem).HasName("PK__DetalleF__51E8426244D4394E");

            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.CodigoProductoNavigation).WithMany(p => p.DetalleFacturas)
                .HasPrincipalKey(p => p.Codigo)
                .HasForeignKey(d => d.CodigoProducto)
                .HasConstraintName("FK__DetalleFa__Codig__3B75D760");

            entity.HasOne(d => d.IdFacturaNavigation).WithMany(p => p.DetalleFacturas).HasConstraintName("FK__DetalleFa__IdFac__3A81B327");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.DetalleFacturas).HasConstraintName("FK__DetalleFa__IdUsu__3D5E1FD2");

           
        });



        modelBuilder.Entity<Factura>(entity =>
        {
            entity.HasKey(e => e.IdFactura).HasName("PK__Facturas__50E7BAF181B25269");

            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Facturas).HasConstraintName("FK__Facturas__IdUsua__37A5467C");
        });

        modelBuilder.Entity<FamiliaProducto>(entity =>
        {
            entity.HasKey(e => e.IdFamilia).HasName("PK__FamiliaP__751F80CF34095A68");

            entity.Property(e => e.Activo).HasDefaultValueSql("((1))");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.FamiliaProductos).HasConstraintName("FK__FamiliaPr__IdUsu__2C3393D0");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.IdProducto).HasName("PK__Producto__09889210DE26F37C");

            entity.Property(e => e.Activo).HasDefaultValueSql("((1))");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.IdFamiliaNavigation).WithMany(p => p.Productos).HasConstraintName("FK__Productos__IdFam__300424B4");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Productos).HasConstraintName("FK__Productos__IdUsu__32E0915F");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__Usuarios__5B65BF97255A1D71");

            entity.Property(e => e.Bloqueado).HasDefaultValueSql("((0))");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IntentosFallidos).HasDefaultValueSql("((0))");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
