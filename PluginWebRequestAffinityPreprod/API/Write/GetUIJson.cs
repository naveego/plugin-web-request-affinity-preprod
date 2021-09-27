using System.Collections.Generic;
using Newtonsoft.Json;

namespace PluginWebRequestAffinityPreprod.API.Write
{
    public static partial class Write
    {
        public static string GetUIJson()
        {
            var uiJsonObj = $@"{{
                ""ui:order"": [
                    ""Name"",
                    ""Method"",
                    ""Url"",
                    ""Body"",
                    ""Headers""
                ],
                ""Headers"": {{
                    ""items"": {{
                        ""ui:order"": [
                            ""Key"",
                            ""Value""
                        ]
                    }}
                }},
                ""Body"": {{
                    ""ui:widget"": ""textarea"",
                    ""ui:options"": {{
                        ""rows"": 5
                    }}
                }}
            }}";
            
            return uiJsonObj;
        }
    }
}