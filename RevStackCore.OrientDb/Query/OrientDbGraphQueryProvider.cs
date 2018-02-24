using System;
using System.Linq;
using System.Linq.Expressions;

namespace RevStackCore.OrientDb.Query
{
    public abstract class OrientDbGraphQueryProvider : IQueryProvider
    {
        protected OrientDbGraphQueryProvider()
        {
        }

        IQueryable<T> IQueryProvider.CreateQuery<T>(Expression expression)
        {
            throw new NotImplementedException();
        }

        IQueryable IQueryProvider.CreateQuery(Expression expression)
        {
            throw new NotImplementedException();
        }

        T IQueryProvider.Execute<T>(Expression expression)
        {
            throw new NotImplementedException();
        }

        object IQueryProvider.Execute(Expression expression)
        {
            throw new NotImplementedException();
        }

        public string QueryText { get; set; }
        public abstract object Execute(string query, Type elementType);
    }
}
