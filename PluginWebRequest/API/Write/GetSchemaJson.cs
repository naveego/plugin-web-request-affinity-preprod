using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using PluginWebRequest.API.Utility;

namespace PluginWebRequest.API.Write
{
    public static partial class Write
    {
        public static string GetSchemaJson()
        {
            var schemaJsonObj = $@"{{
            ""type"": ""object"",
            ""properties"": {{
                ""Name"": {{
                    ""type"": ""string"",
                    ""title"": ""Name"",
                    ""description"": ""The name of the request""
                }},
                ""Method"": {{
                    ""type"": ""string"",
                    ""title"": ""Method"",
                    ""description"": ""The http method of the request"",
                    ""enum"": [
                        ""{Constants.MethodGet}"",
                        ""{Constants.MethodPost}"",
                        ""{Constants.MethodPut}"",
                        ""{Constants.MethodPatch}"",
                        ""{Constants.MethodDelete}""
                    ]
                }},
                ""Url"": {{
                    ""type"": ""string"",
                    ""title"": ""Url"",
                    ""description"": ""The url of the request, supports .NET format strings (ex. https://{{0}}.com), supports Okta token injection (ex. https://aunalytics.com?token={Constants.OktaTokenFind})""
                }},
                ""Body"": {{
                    ""type"": ""string"",
                    ""title"": ""Body"",
                    ""description"": ""The body of the request, supports .NET format strings (ex. {{\""key\"": \""{{0}}\"" }}), supports Okta token injection (ex. {{\""key\"": \""{Constants.OktaTokenFind}\"" }})""
                }},
                ""Headers"": {{
                    ""type"": ""array"",
                    ""title"": ""Headers"",
                    ""description"": ""The headers of the request, supports Okta token injection (ex. Key: Authorization, Value: Bearer {Constants.OktaTokenFind})"",
                    ""items"": {{
                        ""type"": ""object"",
                        ""properties"": {{
                            ""Key"": {{
                                ""type"": ""string"",
                                ""title"": ""Key"",
                                ""description"": ""The key of the header""
                            }},
                            ""Value"": {{
                                ""type"": ""string"",
                                ""title"": ""Value"",
                                ""description"": ""The value of the header""
                            }}
                        }}
                    }}
                }}
            }},
            ""required"": [""Name"",""Method"", ""Url""]
        }}";

            return schemaJsonObj;
        }
    }
}