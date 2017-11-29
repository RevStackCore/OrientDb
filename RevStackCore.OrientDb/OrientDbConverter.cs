using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace RevStackCore.OrientDb
{
    public class OrientDbConverter<TInterface, T> : JsonConverter
        where T : TInterface
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var obj = (T)value;

            //var jObject = GetObject(obj);

            //jObject.WriteTo(writer);
            serializer.Serialize(writer, value);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var item = default(T);
            serializer.Populate(reader, item);
            //var jObject = JObject.Load(reader);

            return (TInterface)item; //GetRectangle(jObject);
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(TInterface[]).GetTypeInfo().IsAssignableFrom(objectType);
        }

        //protected static JObject GetObject(T obj)
        //{
        //    var x = obj.X;
        //    var y = rectangle.Y;
        //    var width = rectangle.Width;
        //    var height = rectangle.Height;

        //    return JObject.FromObject(new { x, y, width, height });
        //}

        //protected static T GetRectangle(JObject jObject)
        //{
        //    var rid = GetTokenStringValue(jObject, "@rid") ?? "#-1:-1";
        //    var version = GetTokenIntValue(jObject, "@version") ?? 0;
            
        //    return new Rectangle(x, y, width, height);
        //}

        //protected static Rectangle GetRectangle(JToken jToken)
        //{
        //    var jObject = JObject.FromObject(jToken);

        //    return GetRectangle(jObject);
        //}

        protected static int? GetTokenIntValue(JObject jObject, string tokenName)
        {
            JToken jToken;
            return jObject.TryGetValue(tokenName, StringComparison.CurrentCultureIgnoreCase, out jToken) ? (int)jToken : (int?)null;
        }

        protected static string GetTokenStringValue(JObject jObject, string tokenName)
        {
            JToken jToken;
            return jObject.TryGetValue(tokenName, StringComparison.CurrentCultureIgnoreCase, out jToken) ? (string)jToken : (string)null;
        }
    }
}
