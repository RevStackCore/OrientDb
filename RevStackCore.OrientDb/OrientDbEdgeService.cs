using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RevStackCore.OrientDb
{
    public class OrientDbEdgeService<TEdge, TIn, TOut, TKey> : IOrientDbEdgeService<TEdge, TIn, TOut, TKey>
        where TEdge : class, IOrientEdgeEntity<TIn, TOut, TKey>
        where TIn : class, IOrientEntity<TKey>
        where TOut : class, IOrientEntity<TKey>

    {
        protected readonly IOrientDbEdgeRepository<TEdge, TIn, TOut, TKey> _repository;

        public OrientDbEdgeService(IOrientDbEdgeRepository<TEdge, TIn, TOut, TKey> repository)
        {
            _repository = repository;
        }

        public virtual IEnumerable<TEdge> Get()
        {
            return _repository.Get();
        }

        public virtual TEdge GetById(TKey id)
        {
            return _repository.GetById(id);
        }

        public virtual Task<IEnumerable<TEdge>> GetAsync()
        {
            return Task.FromResult(Get());
        }

        public virtual Task<TEdge> GetByIdAsync(TKey id)
        {
            return Task.FromResult(GetById(id));
        }

        public virtual IQueryable<TEdge> Find(Expression<Func<TEdge, bool>> predicate)
        {
            return _repository.Find(predicate);
        }

        public virtual Task<IQueryable<TEdge>> FindAsync(Expression<Func<TEdge, bool>> predicate)
        {
            return Task.FromResult(Find(predicate));
        }

        public virtual TEdge Add(TEdge entity)
        {
            return _repository.Add(entity);
        }

        public virtual Task<TEdge> AddAsync(TEdge entity)
        {
            return Task.FromResult(Add(entity));
        }

        public virtual TEdge Update(TEdge entity)
        {
            return _repository.Update(entity);
        }

        public virtual Task<TEdge> UpdateAsync(TEdge entity)
        {
            return Task.FromResult(Update(entity));
        }

        public virtual void Delete(TEdge entity)
        {
            _repository.Delete(entity);
        }

        public virtual Task DeleteAsync(TEdge entity)
        {
            return Task.Run(() => Delete(entity));
        }
    }
}
