using FacturacionAPI1.Models;

namespace FacturacionAPI1.Repository.IRepositorio
{
    public interface IProductoRepositorio : IRepositorio<ProductoRepositorio>
    {
        Task<ProductoRepositorio> Actualizar(ProductoRepositorio entidad);
    }
}
