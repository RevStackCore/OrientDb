using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace RevStackCore.OrientDb
{
    [JsonObject(MemberSerialization.OptOut)]
    public abstract class OrientDbEntity
    {
        [JsonProperty(PropertyName = "@class")]
        public string _class { get { return this.GetType().Name; } }
        [JsonProperty(PropertyName = "@rid")]
        public string _rid { get; set; }
        [JsonProperty(PropertyName = "@version")]
        public int _version { get; set; }
    }
}
