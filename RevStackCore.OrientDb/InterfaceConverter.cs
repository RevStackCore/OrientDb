using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Linq;

namespace RevStackCore.OrientDb
{
    public class InterfaceConverter<TInterface, TConcrete> : CustomCreationConverter<TInterface>
    where TConcrete : TInterface, new()
    {
        public override TInterface Create(Type objectType)
        {
            return new TConcrete();
        }
    }

    public class InterfaceArrayConverter<TInterface, T> : JsonConverter
        where T : TInterface
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(TInterface[]).GetTypeInfo().IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var list = new List<T>();
            serializer.Populate(reader, list);

            //return list.ConvertAll(item => (TInterface)item);
            return list.Select(item => (TInterface)item).ToArray();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}

