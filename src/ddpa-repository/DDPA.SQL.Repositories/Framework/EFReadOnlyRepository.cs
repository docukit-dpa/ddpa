using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using DDPA.SQL.Entities;

namespace DDPA.SQL.Repositories
{
    public class EFReadOnlyRepository<TContext> : IReadOnlyRepository
    where TContext : DbContext
    {
        protected readonly TContext _context;

        public EFReadOnlyRepository(TContext context)
        {
            _context = context;
        }

        private IQueryable<TEntity> GetQueryable<TEntity>(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            int? skip = null, int? take = null, bool asNoTracking = false,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null)

            where TEntity : class, IEntity
        {
            IQueryable<TEntity> query = _context.Set<TEntity>();

            if (filter != null)
                query = query.Where(filter);

            if (include != null)
                query = include(query);

            if (orderBy != null)
                query = orderBy(query);

            if (skip.HasValue)
                query = query.Skip(skip.Value);

            if (take.HasValue)
                query = query.Take(take.Value);

            if (asNoTracking)
                query = query.AsNoTracking();

            return query;
        }

        public virtual IEnumerable<TEntity> GetAll<TEntity>(
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            int? skip = null, int? take = null, bool asNoTracking = false,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null)
            where TEntity : class, IEntity
        {
            return GetQueryable<TEntity>(null, orderBy, skip, take, asNoTracking, include).ToList();
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync<TEntity>(
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            int? skip = null, int? take = null, bool asNoTracking = false,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null)
            where TEntity : class, IEntity
        {
            return await GetQueryable<TEntity>(null, orderBy, skip, take, asNoTracking, include).ToListAsync();
        }

        public virtual IEnumerable<TEntity> Get<TEntity>(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            int? skip = null, int? take = null, bool asNoTracking = false,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null)
            where TEntity : class, IEntity
        {
            return GetQueryable<TEntity>(filter, orderBy, skip, take, asNoTracking, include).ToList();
        }

        public virtual async Task<IEnumerable<TEntity>> GetAsync<TEntity>(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            int? skip = null, int? take = null, bool asNoTracking = false,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null)
            where TEntity : class, IEntity
        {
            return await GetQueryable<TEntity>(filter, orderBy, skip, take, asNoTracking, include).ToListAsync();
        }

        public virtual TEntity GetOne<TEntity>(
            Expression<Func<TEntity, bool>> filter = null, bool asNoTracking = false,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null)
            where TEntity : class, IEntity
        {
            return GetQueryable<TEntity>(filter, null, null, null, asNoTracking, include).SingleOrDefault();
        }

        public virtual async Task<TEntity> GetOneAsync<TEntity>(
            Expression<Func<TEntity, bool>> filter = null, bool asNoTracking = false,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null)
            where TEntity : class, IEntity
        {
            return await GetQueryable<TEntity>(filter, null, null, null, asNoTracking, include).SingleOrDefaultAsync();
        }

        public virtual TEntity GetFirst<TEntity>(
           Expression<Func<TEntity, bool>> filter = null,
           Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, bool asNoTracking = false,
           Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null)
           where TEntity : class, IEntity
        {
            return GetQueryable<TEntity>(filter, orderBy, null, null, asNoTracking, include).FirstOrDefault();
        }

        public virtual async Task<TEntity> GetFirstAsync<TEntity>(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, bool asNoTracking = false,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null)
            where TEntity : class, IEntity
        {
            return await GetQueryable<TEntity>(filter, orderBy, null, null, asNoTracking, include).FirstOrDefaultAsync();
        }

        public virtual TEntity GetById<TEntity>(object id)
            where TEntity : class, IEntity
        {
            return _context.Set<TEntity>().Find(id);
        }

        public virtual Task<TEntity> GetByIdAsync<TEntity>(object id)
            where TEntity : class, IEntity
        {

            return _context.Set<TEntity>().FindAsync(id);
        }

        public virtual int GetCount<TEntity>(Expression<Func<TEntity, bool>> filter = null)
            where TEntity : class, IEntity
        {
            return GetQueryable<TEntity>(filter).Count();
        }

        public virtual Task<int> GetCountAsync<TEntity>(Expression<Func<TEntity, bool>> filter = null)
            where TEntity : class, IEntity
        {
            return GetQueryable<TEntity>(filter).CountAsync();
        }

        public virtual bool GetExists<TEntity>(Expression<Func<TEntity, bool>> filter = null)
            where TEntity : class, IEntity
        {
            return GetQueryable<TEntity>(filter).Any();
        }

        public virtual Task<bool> GetExistsAsync<TEntity>(Expression<Func<TEntity, bool>> filter = null)
            where TEntity : class, IEntity
        {
            return GetQueryable<TEntity>(filter).AnyAsync();
        }
    }
}
