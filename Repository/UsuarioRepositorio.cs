using FacturacionAPI1.Repository.IRepositorio;
using FacturacionAPI1.Models;


namespace FacturacionAPI1.Repository
{
   public class UsuarioRepositorio : Repositorio<Usuario>, IUsuarioRepositorio
    {
        public readonly AplicationDbContext _db;
        public UsuarioRepositorio(AplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<Usuario> Actualizar(Usuario entidad)
        {
            entidad.FechaActualizacion = DateTime.Now;
            _db.Usuarios.Update(entidad);
            await _db.SaveChangesAsync();
            return entidad;
        }
    }
}
