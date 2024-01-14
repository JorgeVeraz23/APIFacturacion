using FacturacionAPI1.Models;
using FacturacionAPI1.Repository.IRepositorio;

namespace FacturacionAPI1.Repository
{
    public class FacturaRepositorio : Repositorio<Factura>, IFacturaRepositorio
    {
        public readonly AplicationDbContext _db;
        public FacturaRepositorio(AplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<Factura> Actualizar(Factura entidad)
        {
            entidad.FechaActualizacion = DateTime.Now;
            _db.Facturas.Update(entidad);
            await _db.SaveChangesAsync();
            return entidad;
        }
    }
}
