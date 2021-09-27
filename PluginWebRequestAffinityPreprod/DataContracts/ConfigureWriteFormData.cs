using System.Collections.Generic;

namespace PluginWebRequestAffinityPreprod.DataContracts
{
    public class ConfigureWriteFormData
    {
        public string Name { get; set; }
        public string Method { get; set; }
        public string Url { get; set; }
        public string Body { get; set; }
        public List<Header>? Headers { get; set; }
    }

    public class Header
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}