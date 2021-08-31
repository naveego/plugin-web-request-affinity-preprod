using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Naveego.Sdk.Plugins;
using Newtonsoft.Json;
using PluginWebRequest.API.Utility;
using PluginWebRequest.DataContracts;

namespace PluginWebRequest.API.Write
{
    public static partial class Write
    {
        private static Regex FindParamsRegex = new Regex(@"\{+(\d)\}");

        public static async Task<Schema> GetSchemaForSettingsAsync(ConfigureWriteFormData formData)
        {
            var schema = new Schema
            {
                Id = formData.Name,
                Name = formData.Name,
                Description = "",
                DataFlowDirection = Schema.Types.DataFlowDirection.Write,
                Query = formData.Url,
                PublisherMetaJson = JsonConvert.SerializeObject(formData)
            };

            var urlParams = FindParamsRegex.Match(formData.Url);
            var bodyParams = FindParamsRegex.Match(formData.Body);

            foreach (var match in urlParams.Captures)
            {
                var property = new Property
                {
                    Id = $"{Constants.UrlPropertyPrefix}_{match}",
                    Name = $"{Constants.UrlPropertyPrefix}_{match}",
                    Description = "",
                    Type = PropertyType.String,
                    TypeAtSource = "",
                };
            
                schema.Properties.Add(property);
            }
            
            foreach (var match in bodyParams.Captures)
            {
                var property = new Property
                {
                    Id = $"{Constants.BodyPropertyPrefix}_{match}",
                    Name = $"{Constants.BodyPropertyPrefix}_{match}",
                    Description = "",
                    Type = PropertyType.String,
                    TypeAtSource = "",
                };
            
                schema.Properties.Add(property);
            }
            
            return schema;
        }
    }
}