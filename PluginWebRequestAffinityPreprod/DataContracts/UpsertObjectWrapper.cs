using System.Collections.Generic;
using Newtonsoft.Json;

namespace PluginWebRequestAffinityPreprod.DataContracts
{
    public class UpsertObjectWrapper
    {
        [JsonProperty("properties")]
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
    }
}