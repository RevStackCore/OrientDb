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

        public virtual IEnumerable<TEntity> Get()
        {
            IQueryable<TEntity> query = new Query.Query<TEntity>(_queryProvider);
            return query.ToList().AsEnumerable<TEntity>();
        }

        public virtual TEntity GetById(TKey id)
        {
            return Find(x => x.Id.Equals(id)).FirstOrDefault();
        }

        public virtual IQueryable<TEntity> Find(Expression<Func<TEntity, bool>> predicate)
        {
            return new Query.Query<TEntity>(_queryProvider).Where(predicate);
        }

        public virtual TEntity Add(TEntity entity)
        {
            //handle vertex objects
            if (entity.GetType() == typeof(IOrientVertexEntity<TKey>))
            {
                var name = entity.GetType().Name;
                _database.Execute("CREATE CLASS " + name + " EXTENDS V");
                _database.Execute("CREATE PROPERTY " + name + ".id STRING");
                _database.Execute("CREATE INDEX " + name + ".id UNIQUE");
            }

            return _database.Insert<TEntity>(entity);
        }

        public virtual TEntity Update(TEntity entity)
        {
            return _database.Update<TEntity>(entity);
        }

        public virtual void Delete(TEntity entity)
        {
            //handle vertex objects
            if (entity.GetType() == typeof(IOrientVertexEntity<TKey>))
            {
                var name = entity.GetType().Name;
                _database.Execute("DELETE VERTEX " + name + " where id = '" + entity.Id.ToString() + "'");
                return;
            }
               
            _database.Delete<TEntity>(entity);
        }
        
    }
}
