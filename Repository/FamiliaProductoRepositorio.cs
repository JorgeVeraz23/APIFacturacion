using FacturacionAPI1.Models;
using FacturacionAPI1.Repository.IRepositorio;

namespace FacturacionAPI1.Repository
{
    public class FamiliaProductoRepositorio : Repositorio<FamiliaProducto>, IFamiliaProductoRepositorio
    {
        public readonly AplicationDbContext _db;
        public FamiliaProductoRepositorio(AplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<FamiliaProducto> Actualizar(FamiliaProducto entidad)
        {
            entidad.FechaActualizacion = DateTime.Now;
            _db.FamiliaProductos.Update(entidad);
            await _db.SaveChangesAsync();
            return entidad;
        }
    }
}
