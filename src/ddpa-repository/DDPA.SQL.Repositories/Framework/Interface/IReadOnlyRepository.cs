using Microsoft.EntityFrameworkCore.Query;
using DDPA.SQL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DDPA.SQL.Repositories
{
    public interface IReadOnlyRepository
    {
        IEnumerable<TEntity> GetAll<TEntity>(
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            int? skip = null,
            int? take = null,
            bool asNoTracking = false,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null)
            where TEntity : class, IEntity;

        Task<IEnumerable<TEntity>> GetAllAsync<TEntity>(
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            int? skip = null,
            int? take = null,
            bool asNoTracking = false,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null)
            where TEntity : class, IEntity;

        IEnumerable<TEntity> Get<TEntity>(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            int? skip = null,
            int? take = null,
            bool asNoTracking = false,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null)
            where TEntity : class, IEntity;

        Task<IEnumerable<TEntity>> GetAsync<TEntity>(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            int? skip = null,
            int? take = null,
            bool asNoTracking = false,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null)
            where TEntity : class, IEntity;

        TEntity GetOne<TEntity>(
            Expression<Func<TEntity, bool>> filter = null, bool asNoTracking = false,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null)
            where TEntity : class, IEntity;

        Task<TEntity> GetOneAsync<TEntity>(
            Expression<Func<TEntity, bool>> filter = null, bool asNoTracking = false,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null)
            where TEntity : class, IEntity;

        TEntity GetFirst<TEntity>(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            bool asNoTracking = false,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null)
            where TEntity : class, IEntity;

        Task<TEntity> GetFirstAsync<TEntity>(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            bool asNoTracking = false,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null)
            where TEntity : class, IEntity;

        TEntity GetById<TEntity>(object id)
            where TEntity : class, IEntity;

        Task<TEntity> GetByIdAsync<TEntity>(object id)
            where TEntity : class, IEntity;

        int GetCount<TEntity>(Expression<Func<TEntity, bool>> filter = null)
            where TEntity : class, IEntity;

        Task<int> GetCountAsync<TEntity>(Expression<Func<TEntity, bool>> filter = null)
            where TEntity : class, IEntity;

        bool GetExists<TEntity>(Expression<Func<TEntity, bool>> filter = null)
            where TEntity : class, IEntity;

        Task<bool> GetExistsAsync<TEntity>(Expression<Func<TEntity, bool>> filter = null)
            where TEntity : class, IEntity;
    }
}
