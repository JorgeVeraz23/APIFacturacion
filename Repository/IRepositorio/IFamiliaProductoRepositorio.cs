using FacturacionAPI1.Models;

namespace FacturacionAPI1.Repository.IRepositorio
{
    public interface IFamiliaProductoRepositorio : IRepositorio<FamiliaProducto>
    {
        Task<FamiliaProducto> Actualizar(FamiliaProducto entidad);
    }
}
