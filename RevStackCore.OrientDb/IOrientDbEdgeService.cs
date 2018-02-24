using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace RevStackCore.OrientDb
{
    public interface IOrientDbEdgeService<TEdge, TIn, TOut, TKey>
    where TEdge : class, IOrientEdgeEntity<TIn, TOut, TKey>
    where TIn : class, IOrientEntity<TKey>
    where TOut : class, IOrientEntity<TKey>
    {
        IEnumerable<TEdge> Get();
        TEdge GetById(TKey id);
        IQueryable<TEdge> Find(Expression<Func<TEdge, bool>> predicate);
        TEdge Add(TEdge entity);
        TEdge Update(TEdge entity);
        void Delete(TEdge entity);
        Task<IEnumerable<TEdge>> GetAsync();
        Task<TEdge> GetByIdAsync(TKey id);
        Task<IQueryable<TEdge>> FindAsync(Expression<Func<TEdge, bool>> predicate);
        Task<TEdge> AddAsync(TEdge entity);
        Task<TEdge> UpdateAsync(TEdge entity);
        Task DeleteAsync(TEdge entity);
    }
}
