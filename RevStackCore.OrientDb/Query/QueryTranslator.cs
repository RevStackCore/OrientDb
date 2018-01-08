using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RevStackCore.OrientDb.Query
{
    internal class TranslateResult
    {
        internal string CommandText;
        internal LambdaExpression Projector;
    }

    internal class QueryTranslator : ExpressionVisitorBase
    {
        StringBuilder sb;
        ParameterExpression row;
        ColumnProjection projection;

        internal QueryTranslator()
        {

        }
        
        Expression root;

        internal TranslateResult Translate(Expression expression)
        {
            root = expression;

            this.sb = new StringBuilder();
            this.row = Expression.Parameter(typeof(ProjectionRow), "row");
            this.Visit(expression);
            return new TranslateResult
            {
                CommandText = this.sb.ToString(),
                Projector = this.projection != null ? Expression.Lambda(this.projection.Selector, this.row) : null
            };
        }

        private static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }
            return e;
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {

            if (m.Method.DeclaringType == typeof(Queryable) ||
                m.Method.DeclaringType == typeof(System.Linq.Enumerable) ||
                m.Method.Name == "Contains" ||
                m.Method.Name == "Equals" || 
                m.Method.Name == "StartsWith" ||
                m.Method.Name == "ToLower" ||
                m.Method.Name == "ToUpper" ||
                m.Method.Name == "Count" || //test this
                m.Method.Name == "Any" || //test this
                m.Method.Name == "ToString")
            {
                int resultInt;
                long resultLong;
                double resultDouble;
                float resultFloat;
                decimal resultdecimal;

                if (m.Method.Name == "Where")
                {
                    //sb.Append("SELECT * FROM (");
                    this.Visit(m.Arguments[0]);
                    sb.Append(" WHERE ");
                    LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                    this.Visit(lambda.Body);
                    return m;
                }
                else if (m.Method.Name == "Single" || m.Method.Name == "SingleOrDefault")
                {
                    this.Visit(m.Arguments[0]);
                    if (!sb.ToString().Contains("WHERE"))
                        sb.Append(" WHERE ");
                    if (m.Arguments.Count > 1)
                    {
                        LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                        this.Visit(lambda.Body);
                    }
                    //this.Visit(m.Arguments[1]);
                    sb.Append(" LIMIT 1 ");
                    return m;
                }
                else if (m.Method.Name == "FirstOrDefault")
                {
                    this.Visit(m.Arguments[0]);
                    if (!sb.ToString().Contains("WHERE"))
                        sb.Append(" WHERE ");
                    if (m.Arguments.Count > 1)
                    {
                        LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                        this.Visit(lambda.Body);
                    }
                    //this.Visit(m.Arguments[1]);
                    sb.Append(" LIMIT 1 ");
                    return m;
                }
                else if (m.Method.Name == "Count" || m.Method.Name == "Any")
                {
                    this.Visit(m.Arguments[0]);
                    if (!sb.ToString().Contains("WHERE"))
                        sb.Append(" WHERE ");
                    if (m.Arguments.Count > 1)
                    {
                        LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                        this.Visit(lambda.Body);
                    }
                    return m;
                }
                else if (m.Method.Name == "Contains")
                {
                    if (m.Arguments[0].NodeType == ExpressionType.MemberAccess)
                    {
                        MemberExpression exp = (MemberExpression)m.Arguments[0];
                        object value = GetValue(exp);
                        var memberExpression = (MemberExpression)m.Object;
                        string param = ToCamelCase(memberExpression.Member.Name);
                        string v = " " + param + " LIKE \'%" + value.ToString() + "%\'";
                        sb.Append(v);
                    }
                    else
                    {
                        var exp = (ConstantExpression)m.Arguments[0];
                        var memberExpression = (MemberExpression)m.Object;
                        string param = ToCamelCase(memberExpression.Member.Name);
                        object value = GetValue(exp);
                        string v = " " + param + " LIKE \'%" + value.ToString() + "%\'";
                        sb.Append(v);
                    }

                    return m;
                }
                else if (m.Method.Name == "StartsWith")
                {
                    if (m.Arguments[0].NodeType == ExpressionType.MemberAccess)
                    {
                        MemberExpression exp = (MemberExpression)m.Arguments[0];
                        object value = GetValue(exp);
                        var memberExpression = (MemberExpression)m.Object;
                        string param = ToCamelCase(memberExpression.Member.Name);
                        //string param = exp.Member.Name;
                        string v = " " + param + " LIKE \'" + value.ToString() + "%\'";
                        sb.Append(v);
                    }
                    else
                    {
                        var exp = (ConstantExpression)m.Arguments[0];
                        var memberExpression = (MemberExpression)m.Object;
                        string param = ToCamelCase(memberExpression.Member.Name);
                        object value = GetValue(exp);
                        string v = " " + param + " LIKE \'" + value.ToString() + "%\'";
                        sb.Append(v);
                    }

                    return m;
                }
                else if (m.Method.Name == "ToString")
                {
                    if (m.Arguments.Count() == 0)
                    {
                        var memberExpression = (MemberExpression)m.Object;
                        if (memberExpression.Expression.NodeType == ExpressionType.Parameter)
                        {
                            string param = ToCamelCase(memberExpression.Member.Name);
                            string v = " " + param;
                            sb.Append(v);
                        }
                        else if (memberExpression != null && memberExpression.Expression.NodeType == ExpressionType.Constant || memberExpression.Expression.NodeType == ExpressionType.MemberAccess)
                        {
                            object v = GetValue(m);

                            if (v == null)
                                v = "";

                            if (v.GetType() == typeof(string))
                            {
                                v = "'" + v.ToString() + "'";
                            }
                            sb.Append(v);
                        }
                    }

                    return m;
                }
                else if (m.Method.Name == "ToLower")
                {

                    if (m.Arguments.Count() == 0)
                    {
                        var memberExpression = (MemberExpression)m.Object;
                        if (memberExpression.Expression.NodeType == ExpressionType.Parameter)
                        {
                            string param = ToCamelCase(memberExpression.Member.Name);
                            string v = " " + param + ".toLowerCase()";
                            sb.Append(v);
                        }
                        else if (memberExpression != null && memberExpression.Expression.NodeType == ExpressionType.Constant || memberExpression.Expression.NodeType == ExpressionType.MemberAccess)
                        {
                            object v = GetValue(m);

                            if (v == null)
                                v = "";

                            if (v.GetType() == typeof(string))
                            {
                                v = "'" + v.ToString().ToLower() + "'";
                            }
                            sb.Append(v);
                        }
                    }

                    return m;
                }
                else if (m.Method.Name == "ToUpper")
                {
                    if (m.Arguments.Count() == 0)
                    {
                        var memberExpression = (MemberExpression)m.Object;
                        if (memberExpression.Expression.NodeType == ExpressionType.Parameter)
                        {
                            string param = ToCamelCase(memberExpression.Member.Name);
                            string v = " " + param + ".toUpperCase()";
                            sb.Append(v);
                        }
                        else if (memberExpression != null && memberExpression.Expression.NodeType == ExpressionType.Constant || memberExpression.Expression.NodeType == ExpressionType.MemberAccess)
                        {
                            object v = GetValue(m);

                            if (v == null)
                                v = "";

                            if (v.GetType() == typeof(string))
                            {
                                v = "'" + v.ToString().ToUpper() + "'";
                            }
                            sb.Append(v);
                        }
                    }

                    return m;
                }
                else if (m.Method.Name == "Equals")
                {
                    

                    if (m.Arguments[0].NodeType == ExpressionType.MemberAccess)
                    {
                        MemberExpression exp = (MemberExpression)m.Arguments[0];
                        var memberExpression = (MemberExpression)m.Object;
                        string param = memberExpression.Member.Name;
                        var value = GetValue(exp).ToString();
                        string v = " " + param + " = '" + value + "'";
                        if (int.TryParse(value, out resultInt) || long.TryParse(value, out resultLong) || double.TryParse(value, out resultDouble) || float.TryParse(value, out resultFloat) || decimal.TryParse(value, out resultdecimal))
                        {
                            v = " " + param + " = " + value.ToString();
                        }
                        sb.Append(v);
                    }
                    else if (m.Arguments[0].NodeType == ExpressionType.Convert)
                    {
                        var exp = m.Arguments[0];
                        var memberExpression = (MemberExpression)m.Object;
                        string param = ToCamelCase(memberExpression.Member.Name);
                        var value = GetValue(exp).ToString();
                        string v = " " + param + " = '" + value + "'";
                        if (int.TryParse(value, out resultInt) || long.TryParse(value, out resultLong) || double.TryParse(value, out resultDouble) || float.TryParse(value, out resultFloat) || decimal.TryParse(value, out resultdecimal))
                        {
                            v = " " + param + " = " + value.ToString();
                        }
                        sb.Append(v);
                    }
                    else
                    {
                        var exp = (ConstantExpression)m.Arguments[0];
                        var memberExpression = (MemberExpression)m.Object;
                        string param = ToCamelCase(memberExpression.Member.Name);
                        var value = GetValue(exp).ToString();
                        string v = " " + param + " = '" + value + "'";
                        if (int.TryParse(value, out resultInt) || long.TryParse(value, out resultLong) || double.TryParse(value, out resultDouble) || float.TryParse(value, out resultFloat) || decimal.TryParse(value, out resultdecimal))
                        {
                            v = " " + param + " = " + value.ToString();
                        }
                        sb.Append(v);
                    }

                    return m;
                }
                else if (m.Method.Name == "OrderBy")
                {
                    this.Visit(m.Arguments[0]);
                    if (!sb.ToString().Contains("ORDER BY"))
                        sb.Append(" ORDER BY ");
                    if (m.Arguments.Count > 1)
                    {
                        LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                        this.Visit(lambda.Body);
                    }
                    return m;
                }
                else if (m.Method.Name == "OrderByDescending")
                {
                    this.Visit(m.Arguments[0]);
                    if (!sb.ToString().Contains("ORDER BY DESC"))
                        sb.Append(" ORDER BY DESC ");
                    if (m.Arguments.Count > 1)
                    {
                        LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                        this.Visit(lambda.Body);
                    }
                    return m;
                }
                else if (m.Method.Name == "Select")
                {
                    LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                    ColumnProjection projection = new ColumnProjector().ProjectColumns(lambda.Body, this.row);
                    sb.Append("SELECT ");
                    sb.Append(projection.Columns);
                    sb.Append(" FROM (");
                    this.Visit(m.Arguments[0]);
                    sb.Append(") ");
                    this.projection = projection;
                    return m;
                }
            }


            try
            {
                //handle custom extensions
                this.Visit(m.Arguments[1]);
                sb.Append(" = ");
                this.Visit(m.Arguments[2]);
                return m;
            }
            catch (Exception)
            {
                throw new NotSupportedException(string.Format("The method '{0}' is not supported", m.Method.Name));
            }
            
        }


        #region "binds mapping"

        private MemberInfo GetMethodInfo<T>(Expression<Func<T, Delegate>> expression)
        {
            var unaryExpression = (UnaryExpression)expression.Body;
            var methodCallExpression = (MethodCallExpression)unaryExpression.Operand;
            var methodInfoExpression = (ConstantExpression)methodCallExpression.Arguments.Last();
            var methodInfo = (MemberInfo)methodInfoExpression.Value;
            return methodInfo;
        }

        private bool IsQuery(Expression expression)
        {
            return
                expression.Type.GetTypeInfo().IsGenericType
                && typeof(IQueryable<>).MakeGenericType(expression.Type.GetGenericArguments()).IsAssignableFrom(expression.Type);
        }
        #endregion

        protected override Expression VisitUnary(UnaryExpression u)
        {
            if (u.NodeType == ExpressionType.ArrayLength)
            {
                Expression expression = this.Visit(u.Operand);
                //translate arraylength into normal member expression
                return Expression.MakeMemberAccess(expression, expression.Type.GetRuntimeProperty("Length"));
            }
            else if (u.NodeType == ExpressionType.Convert)
            {
                return base.Visit(u.Operand);
            }
            else
            {
                return u.Update(this.Visit(u.Operand));
            }

            switch (u.NodeType)
            {
                case ExpressionType.Not:
                    sb.Append(" NOT ");
                    this.Visit(u.Operand);
                    break;
                default:
                    throw new NotSupportedException(string.Format("The unary operator '{0}' is not supported", u.NodeType));
            }
            return u;
        }

        protected override Expression VisitBinary(BinaryExpression b)
        {
            sb.Append("(");
            this.Visit(b.Left);
            switch (b.NodeType)
            {
                case ExpressionType.And:
                    sb.Append(" AND ");
                    break;
                case ExpressionType.AndAlso:
                    sb.Append(" AND ");
                    break;
                case ExpressionType.Or:
                    sb.Append(" OR ");
                    break;
                case ExpressionType.OrElse:
                    sb.Append(" OR ");
                    break;
                case ExpressionType.Equal:
                    sb.Append(" = ");
                    break;
                case ExpressionType.NotEqual:
                    sb.Append(" <> ");
                    break;
                case ExpressionType.LessThan:
                    sb.Append(" < ");
                    break;
                case ExpressionType.LessThanOrEqual:
                    sb.Append(" <= ");
                    break;
                case ExpressionType.GreaterThan:
                    sb.Append(" > ");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    sb.Append(" >= ");
                    break;
                default:
                    throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported", b.NodeType));
            }
            this.Visit(b.Right);
            sb.Append(")");
            return b;
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            IQueryable q = c.Value as IQueryable;
            if (q != null)
            {
                // assume constant nodes w/ IQueryables are table references
                sb.Append("SELECT FROM ");
                sb.Append(q.ElementType.Name);
                //if (q.ElementType.GetProperty("_class") != null)
                //{
                //    PropertyInfo info = q.ElementType.GetProperty("_class");
                //    var el = q.ElementType;
                //    object val = info.GetValue(el);
                //    sb.Append(val.ToString());
                //}
                //else
                //{
                //    sb.Append(q.ElementType.Name);
                //} 
            }
            else if (c.Value == null)
            {
                sb.Append("NULL");
            }
            else
            {
                switch (Type.GetTypeCode(c.Value.GetType()))
                {
                    case TypeCode.Boolean:
                        sb.Append(((bool)c.Value) ? 1 : 0);
                        break;
                    case TypeCode.String:
                        sb.Append("'");
                        sb.Append(c.Value);
                        sb.Append("'");
                        break;
                    case TypeCode.Object:
                        throw new NotSupportedException(string.Format("The constant for '{0}' is not supported", c.Value));
                    default:
                        sb.Append(c.Value);
                        break;
                }
            }
            return c;
        }

        //DEFAULT To LOWER CASE FOR ALL MEMBERS
        protected override Expression VisitMemberAccess(MemberExpression m)
        {
            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter)
            {
                //camel case all properties
                sb.Append(ToCamelCase(m.Member.Name));
                return m;
            } //there must be a better way...
            else if (m.Expression != null && m.Expression.NodeType == ExpressionType.Constant || m.Expression.NodeType == ExpressionType.MemberAccess)
            {
                object v = GetValue(m);
                //handle strings...
                if (v == null)
                    v = "null";
                if (m.Member.Name == "Contains" && v != null && v.GetType() == typeof(string))
                {
                    v = "\'%" + v.ToString() + "%\'";
                }
                else if (v != null && v.GetType() == typeof(string))
                {
                    v = "\'" + v.ToString() + "\'";
                }
                sb.Append(v);
                return m;
            }
            throw new NotSupportedException(string.Format("The member '{0}' is not supported", m.Member.Name));
        }

        public static object GetValue(Expression member)
        {
            return Expression.Lambda(member).Compile().DynamicInvoke();
        }

        public static string ToCamelCase(string s)
        {
            if (string.IsNullOrEmpty(s) || !char.IsUpper(s[0]))
            {
                return s;
            }

            char[] chars = s.ToCharArray();

            for (int i = 0; i < chars.Length; i++)
            {
                if (i == 1 && !char.IsUpper(chars[i]))
                {
                    break;
                }

                bool hasNext = (i + 1 < chars.Length);
                if (i > 0 && hasNext && !char.IsUpper(chars[i + 1]))
                {
                    break;
                }

#if !(DOTNET || PORTABLE)
                chars[i] = char.ToLower(chars[i]);
#else
                chars[i] = char.ToLowerInvariant(chars[i]);
#endif
            }

            return new string(chars);
        }

        public static string ToCamelCase2(string s)
        {
            //if (string.IsNullOrEmpty(s))
            //    return s;

            //if (!char.IsUpper(s[0]))
            //    return s;

            //string camelCase = char.ToLower(s[0], CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
            //if (s.Length > 1)
            //    camelCase += s.Substring(1);

            //return camelCase;

            // If there are 0 or 1 characters, just return the string.
            if (s == null || s.Length < 2)
                return s;

            // Split the string into words.
            string[] words = s.Split(
                new char[] { },
                StringSplitOptions.RemoveEmptyEntries);

            // Combine the words.
            string result = words[0].ToLower();
            for (int i = 1; i < words.Length; i++)
            {
                result +=
                    words[i].Substring(0, 1).ToUpper() +
                    words[i].Substring(1);
            }

            return result;
        }

    }
}
