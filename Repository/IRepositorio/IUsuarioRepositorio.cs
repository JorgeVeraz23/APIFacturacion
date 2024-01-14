

using FacturacionAPI1.Models;

namespace FacturacionAPI1.Repository.IRepositorio
{
    public interface IUsuarioRepositorio : IRepositorio<Usuario>
    {
        Task<Usuario> Actualizar(Usuario entidad);

    }
}
