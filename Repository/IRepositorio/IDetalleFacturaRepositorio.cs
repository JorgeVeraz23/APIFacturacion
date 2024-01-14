using FacturacionAPI1.Models;

namespace FacturacionAPI1.Repository.IRepositorio
{
    public interface IDetalleFacturaRepositorio : IRepositorio<Factura>
    {
        Task<DetalleFactura> Actualizar(DetalleFactura entidad);
    }
}
