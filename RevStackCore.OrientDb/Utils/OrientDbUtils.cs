using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace RevStackCore.OrientDb.Utils
{
    public static class OrientDbUtils
    {
        private const string ID_PROPERTY = "Id";

        public static TEntity SetEntityIdProperty<TEntity>(TEntity entity)
        {
            Type type = entity.GetType();
            var info = type.GetProperty(ID_PROPERTY);
            var value = info.GetValue(entity, null);
            if (info.PropertyType == typeof(int))
            {
                if ((int)value == default(int))
                {
                    info.SetValue(entity, int.Parse(GetUniqueId()));
                }
            }
            else if (info.PropertyType == typeof(long))
            {
                if ((long)value == default(long))
                {
                    info.SetValue(entity, long.Parse(GetUniqueId()));
                }
            }
            else if (info.PropertyType == typeof(float))
            {
                if ((float)value == default(float))
                {
                    info.SetValue(entity, float.Parse(GetUniqueId()));
                }
            }
            else if (info.PropertyType == typeof(Guid))
            {
                if ((Guid)value == default(Guid))
                {
                    info.SetValue(entity, Guid.NewGuid());
                }
            }
            else if (info.PropertyType == typeof(String))
            {
                if ((String)value == default(String))
                {
                    info.SetValue(entity, GetUniqueId());
                }
            }

            return entity;
        }

        public static string GetEntityIdQueryFormat<TEntity>(TEntity entity)
        {
            string format = "";
            string idLowerCase = ID_PROPERTY.ToLower();

            Type type = entity.GetType();
            var info = type.GetProperty(ID_PROPERTY);
            var value = info.GetValue(entity, null);
            if (info.PropertyType == typeof(int))
            {
                format = idLowerCase + " = " + value;
            }
            else if (info.PropertyType == typeof(long))
            {
                format = idLowerCase + " = " + value;
            }
            else if (info.PropertyType == typeof(float))
            {
                format = idLowerCase + " = " + value;
            }
            else if (info.PropertyType == typeof(Guid))
            {
                format = idLowerCase + " = '" + value + "'";
            }
            else if (info.PropertyType == typeof(String))
            {
                format = idLowerCase + " = '" + value + "'";
            }

            return format;
        }

        public static string GetEntityIdType<TEntity>(TEntity entity)
        {
            string format = "";

            Type type = entity.GetType();
            var info = type.GetProperty(ID_PROPERTY);
            var value = info.GetValue(entity, null);

            if (info.PropertyType == typeof(int))
            {
                format = "INTEGER";
            }
            else if (info.PropertyType == typeof(long))
            {
                format = "LONG";
            }
            else if (info.PropertyType == typeof(float))
            {
                format = "FLOAT";
            }
            else if (info.PropertyType == typeof(Guid))
            {
                format = "STRING";
            }
            else if (info.PropertyType == typeof(String))
            {
                format = "STRING";
            }

            return format;
        }

        public static string GetEntityIdJToken(JToken idToken)
        {
            string format = "";

            JTokenType tokenType = idToken.Type;
            Type type = tokenType.GetType();
            string id = idToken.ToString();

            if (tokenType == JTokenType.Integer)
            {
                format = "INTEGER";
            }
            else if (tokenType == JTokenType.Float)
            {
                format = "FLOAT";
            }
            else if (tokenType == JTokenType.Guid)
            {
                format = "STRING";
            }
            else if (tokenType == JTokenType.String)
            {
                format = "STRING";
            }

            return format;
        }

        public static string GetEntityIdQueryFormat(JToken idToken)
        {
            string format = "";
            string idLowerCase = ID_PROPERTY.ToLower();

            JTokenType tokenType = idToken.Type;
            Type type = tokenType.GetType();
            string id = idToken.ToString();

            if (tokenType == JTokenType.Integer)
            {
                format = idLowerCase + " = " + id;
            }
            else if (tokenType == JTokenType.Float)
            {
                format = idLowerCase + " = " + id;
            }
            else if (tokenType == JTokenType.Guid)
            {
                format = idLowerCase + " = '" + id + "'";
            }
            else if (tokenType == JTokenType.String)
            {
                format = idLowerCase + " = '" + id + "'";
            }

            return format;
        }

        private static string GetUniqueId()
        {
            var bytes = new byte[4];
            var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            uint random = BitConverter.ToUInt32(bytes, 0) % 100000000;
            return String.Format("{0:D8}", random);
        }
    }
}
