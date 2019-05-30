using Microsoft.EntityFrameworkCore;
using DDPA.SQL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DDPA.SQL.Repositories
{
    public class EFRepository<TContext> : EFReadOnlyRepository<TContext>, IRepository
    where TContext : DbContext
    {
        public EFRepository(TContext context)
            : base(context)
        {
        }

        public virtual void Create<TEntity>(TEntity entity)
            where TEntity : class, IEntity
        {
            _context.Set<TEntity>().Add(entity);
        }

        public virtual void Update<TEntity>(TEntity entity)
            where TEntity : class, IEntity
        {
            _context.Set<TEntity>().Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        public virtual void Delete<TEntity>(object id)
            where TEntity : class, IEntity
        {
            TEntity entity = _context.Set<TEntity>().Find(id);
            Delete(entity);
        }

        public virtual void Delete<TEntity>(TEntity entity)
            where TEntity : class, IEntity
        {
            var dbSet = _context.Set<TEntity>();
            if (_context.Entry(entity).State == EntityState.Detached)
            {
                dbSet.Attach(entity);
            }

            dbSet.Remove(entity);
        }

        public virtual void Save()
        {
            _context.SaveChanges();
        }

        public virtual Task SaveAsync()
        {
            return _context.SaveChangesAsync();
        }
    }
}
