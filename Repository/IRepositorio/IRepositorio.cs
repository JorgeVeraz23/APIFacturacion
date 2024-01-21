using System.Linq.Expressions;
using FacturacionAPI1.Repository.IRepositorio;
using FacturacionAPI1.Repository;
namespace FacturacionAPI1.Repository.IRepositorio
{
    public interface IRepositorio<T> where T : class
    {

        Task Crear(T entidad);
        Task<List<T>> ObtenerTodos(Expression<Func<T, bool>>? filtro = null);
        Task<T> Obtener(Expression<Func<T, bool>> filtro = null, bool tracked = true);
        Task Remover(T entidad);
        Task Grabar();
        Task<T> ObtenerUsuarioPorCredenciales(string usuario, string contraseña);

    }
}