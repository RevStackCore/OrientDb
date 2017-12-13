using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RevStackCore.OrientDb.Query
{
    public abstract class QueryProvider : IQueryProvider
    {

        protected QueryProvider()
        {
        }

        IQueryable<T> IQueryProvider.CreateQuery<T>(Expression expression)
        {
            return new Query<T>(this, expression);
        }

        IQueryable IQueryProvider.CreateQuery(Expression expression)
        {
            Type elementType = TypeSystem.GetElementType(expression.Type);
            try
            {
                return (IQueryable)Activator.CreateInstance(typeof(Query<>).MakeGenericType(elementType), new object[] { this, expression });
            }
            catch (TargetInvocationException tie)
            {
                throw tie.InnerException;
            }
        }

        T IQueryProvider.Execute<T>(Expression expression)
        {
            Type elementType = TypeSystem.GetElementType(expression.Type);
            if (elementType == typeof(Int32) || elementType == typeof(bool))
            {
                return (T)this.Execute(expression);
            }
            //Single or SingleOrDefault
            IEnumerable results = (IEnumerable)this.Execute(expression);
            IEnumerable<T> en = results.Cast<T>();
            return en.FirstOrDefault<T>();
        }

        object IQueryProvider.Execute(Expression expression)
        {
            return this.Execute(expression);
        }

        protected string GetQueryText(Expression expression)
        {
            return this.Translate(expression);
        }

        protected string Translate(Expression expression)
        {
            return new QueryTranslator().Translate(expression).CommandText;  
        }

        public abstract object Execute(Expression expression);
    }
}
