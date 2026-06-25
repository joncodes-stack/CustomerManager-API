using CustomerManager.Domain.Interfaces.Repositories;
using CustomerManager.Infra.Database;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;


namespace CustomerManager.Infra.Repositories
{
    public abstract class BaseRepository<T>(CustomerContext context) : IBaseRepository<T> where T : class
    {
        protected readonly CustomerContext _context = context;

        public void Add(T entity) => _context.Set<T>().Add(entity);
        public void Update(T entity) => _context.Set<T>().Update(entity);
        public void Delete(T entity) => _context.Set<T>().Remove(entity);

        public async Task<T?> GetByIdAsync(Guid id) => await _context.Set<T>().FindAsync(id);
        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

        public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        {
            IQueryable<T> query = _context.Set<T>();
            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            return await query.CountAsync();
        }

        public async Task<IReadOnlyList<T>> GetPagedAsync(int pagina, int tamanhoPagina, Expression<Func<T, bool>>? predicate = null)
        {
            IQueryable<T> query = _context.Set<T>();
            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            return await query
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .ToListAsync();
        }
    }
}
