using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using Naveego.Sdk.Logging;
using PluginWebRequest.API.Utility;
using PluginWebRequest.DataContracts;
using PluginWebRequest.Helper;

namespace PluginWebRequest.API.Factory
{
    public class ApiClient: IApiClient
    {
        private IApiAuthenticator Authenticator { get; set; }
        private static HttpClient Client { get; set; }
        private ConfigureWriteFormData Settings { get; set; }
        
        public ApiClient(HttpClient client, ConfigureWriteFormData settings)
        {
            Authenticator = new ApiAuthenticator(client, settings);
            Client = client;
            Settings = settings;
            
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            foreach (var header in Settings.Headers)
            {
                Client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        }
        
        public async Task TestConnection()
        {
            try
            {
                var token = await Authenticator.GetToken();
                var uriBuilder = new UriBuilder(Utility.Constants.TestConnectionPath);
                var uri = new Uri(uriBuilder.ToString());
                
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = uri,
                };
                
                var response = await Client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception(await response.Content.ReadAsStringAsync());
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw;
            }
        }

        public async Task<HttpResponseMessage> GetAsync(string path)
        {
            try
            {
                var token = await Authenticator.GetToken();
                var uriBuilder = new UriBuilder(path);

                var uri = new Uri(uriBuilder.ToString());
                
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = uri,
                };

                return await Client.SendAsync(request);
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw;
            }
        }

        public async Task<HttpResponseMessage> PostAsync(string path, StringContent json)
        {
            try
            {
                var token = await Authenticator.GetToken();
                var uriBuilder = new UriBuilder(path);

                var uri = new Uri(uriBuilder.ToString());
                
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = uri,
                    Content = json
                };
                
                return await Client.SendAsync(request);
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw;
            }
        }

        public async Task<HttpResponseMessage> PutAsync(string path, StringContent json)
        {
            try
            {
                var token = await Authenticator.GetToken();
                var uriBuilder = new UriBuilder(path);

                var uri = new Uri(uriBuilder.ToString());
                
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Put,
                    RequestUri = uri,
                    Content = json
                };

                return await Client.SendAsync(request);
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw;
            }
        }

        public async Task<HttpResponseMessage> PatchAsync(string path, StringContent json)
        {
            try
            {
                var token = await Authenticator.GetToken();
                var uriBuilder = new UriBuilder(path);
                
                var uri = new Uri(uriBuilder.ToString());
                
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Patch,
                    RequestUri = uri,
                    Content = json
                };

                return await Client.SendAsync(request);
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw;
            }
        }

        public async Task<HttpResponseMessage> DeleteAsync(string path)
        {
            try
            {
                var token = await Authenticator.GetToken();
                var uriBuilder = new UriBuilder(path);

                var uri = new Uri(uriBuilder.ToString());
                
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Delete,
                    RequestUri = uri
                };

                return await Client.SendAsync(request);
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw;
            }
        }
    }
}