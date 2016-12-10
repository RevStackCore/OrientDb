using System;
using System.Collections.Generic;
using System.Linq;
using RevStackCore.Pattern;
using System.Linq.Expressions;
using RevStackCore.OrientDb.Client;

namespace RevStackCore.OrientDb
{
    public class OrientDbRepository<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>
    {
        private readonly OrientDbDatabase _database;
        private readonly HttpQueryProvider _queryProvider;

        public OrientDbRepository(OrientDbContext context)
        {
            _database = context.Database;
            _queryProvider = new HttpQueryProvider(context.Connection);
        }

        public IEnumerable<TEntity> Get()
        {
            IQueryable<TEntity> query = new Query.Query<TEntity>(_queryProvider);
            return query.ToList().AsEnumerable<TEntity>();
        }

        public IQueryable<TEntity> Find(Expression<Func<TEntity, bool>> predicate)
        {
            return new Query.Query<TEntity>(_queryProvider).Where(predicate);
        }

        public TEntity Add(TEntity entity)
        {
            return _database.Insert<TEntity>(entity);
        }

        public TEntity Update(TEntity entity)
        {
            return _database.Update<TEntity>(entity);
        }

        public void Delete(TEntity entity)
        {
            _database.Delete<TEntity>(entity);
        }
        
    }
}
