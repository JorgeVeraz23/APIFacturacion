using FacturacionAPI1.Models;

namespace FacturacionAPI1.Repository.IRepositorio
{
    public interface IProductoRepositorio : IRepositorio<Producto>
    {
        Task<Producto> Actualizar(Producto entidad);
    }
}
