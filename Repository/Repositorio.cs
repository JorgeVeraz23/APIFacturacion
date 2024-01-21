using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Linq;

using FacturacionAPI1.Repository;
using FacturacionAPI1.Models;
using FacturacionAPI1.Repository.IRepositorio;

namespace FacturacionAPI1.Repository
{
    public class Repositorio<T> : IRepositorio<T> where T : class
    {

        private readonly AplicationDbContext _db;
        internal DbSet<T> dbSet;

        public Repositorio(AplicationDbContext db)
        {
            _db = db;
            this.dbSet = _db.Set<T>();
        }
        public async Task Crear(T entidad)
        {
            await dbSet.AddAsync(entidad);
            await Grabar();
        }

        public async Task<T> ObtenerUsuarioPorCredenciales(string usuario, string contraseña)
        {
            // Asegúrate de que la entidad T tenga las propiedades Usuario1 y Contraseña
            return await dbSet.FirstOrDefaultAsync(u => EF.Property<string>(u, "Usuario1") == usuario && EF.Property<string>(u, "Contraseña") == contraseña);
        }

        public async Task Grabar()
        {
            await _db.SaveChangesAsync();
        }
        public async Task<T> Obtener(Expression<Func<T, bool>> filtro = null, bool tracked = true)
        {
            IQueryable<T> query = dbSet;
            if (!tracked)
            {
                query = query.AsNoTracking();
            }
            if (filtro != null)
            {
                query = query.Where(filtro);
            }
            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<T>> ObtenerTodos(Expression<Func<T, bool>>? filtro = null)
        {
            IQueryable<T> query = dbSet;
            if (filtro != null)
            {
                query = query.Where(filtro);
            }
            return await query.ToListAsync();
        }

        public async Task Remover(T entidad)
        {
            dbSet.Remove(entidad);
            await Grabar();
        }

        



    }
}
