using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace RevStackCore.OrientDb.Query
{
    public class GraphQuery<T> : IQueryable<T>, IQueryable, IEnumerable<T>, IEnumerable, IOrderedQueryable<T>, IOrderedQueryable
    {
        OrientDbGraphQueryProvider provider;
        string query;

        public GraphQuery(OrientDbGraphQueryProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("Provider");
            }
            this.provider = provider;
            this.query = provider.QueryText;
        }

        public Expression Expression
        {
            get { return null; }
        }

        public Type ElementType
        {
            get { return typeof(T); }
        }

        public IQueryProvider Provider
        {
            get { return this.provider; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            IEnumerable results = (IEnumerable)this.provider.Execute(QueryText, ElementType);
            IEnumerator<T> en = results.Cast<T>().GetEnumerator();
            return en;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.provider.Execute(QueryText, ElementType)).GetEnumerator();
        }

        public override string ToString()
        {
            return query;
        }

        public string QueryText
        {
            get
            {
                return query;
            }
        }
    }
}
