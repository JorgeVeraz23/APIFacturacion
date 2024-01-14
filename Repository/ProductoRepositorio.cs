using FacturacionAPI1.Models;
using FacturacionAPI1.Repository.IRepositorio;

namespace FacturacionAPI1.Repository
{
    public class ProductoRepositorio : Repositorio<Producto>, IProductoRepositorio
    {
        public readonly AplicationDbContext _db;
        public ProductoRepositorio(AplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<Producto> Actualizar(Producto entidad)
        {
            entidad.FechaActualizacion = DateTime.Now;
            _db.Productos.Update(entidad);
            await _db.SaveChangesAsync();
            return entidad;
        }
    }
}
