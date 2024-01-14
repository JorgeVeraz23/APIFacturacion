using FacturacionAPI1.Models;
using FacturacionAPI1.Repository.IRepositorio;

namespace FacturacionAPI1.Repository
{
    public class DetalleFacturaRepositorio : Repositorio<DetalleFactura>, IDetalleFacturaRepositorio
    {
        public readonly AplicationDbContext _db;
        public DetalleFacturaRepositorio(AplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<DetalleFactura> Actualizar(DetalleFactura entidad)
        {
            entidad.FechaActualizacion = DateTime.Now;
            _db.DetalleFacturas.Update(entidad);
            await _db.SaveChangesAsync();
            return entidad;
        }
    }
}
