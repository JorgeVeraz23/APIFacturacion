using FacturacionAPI1.Models;

namespace FacturacionAPI1.Repository.IRepositorio
{
    public interface IFacturaRepositorio : IRepositorio<Factura>
    {
        Task<Factura> Actualizar(Factura entidad);
    }
}
