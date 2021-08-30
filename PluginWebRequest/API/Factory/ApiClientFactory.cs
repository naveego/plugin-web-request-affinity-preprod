using System.Net.Http;
using PluginWebRequest.DataContracts;
using PluginWebRequest.Helper;

namespace PluginWebRequest.API.Factory
{
    public class ApiClientFactory: IApiClientFactory
    {
        private HttpClient Client { get; set; }

        public ApiClientFactory(HttpClient client)
        {
            Client = client;
        }

        public IApiClient CreateApiClient(ConfigureWriteFormData settings)
        {
            return new ApiClient(Client, settings);
        }
    }
}