using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using Naveego.Sdk.Logging;
using PluginWebRequestAffinityPreprod.API.Utility;
using PluginWebRequestAffinityPreprod.DataContracts;
using PluginWebRequestAffinityPreprod.Helper;

namespace PluginWebRequestAffinityPreprod.API.Factory
{
    public class ApiClient: IApiClient
    {
        private IApiAuthenticator Authenticator { get; set; }
        private static HttpClient Client { get; set; }
        private ConfigureWriteFormData Settings { get; set; }
        private Settings ConnectSettings { get; set; }
        
        public ApiClient(HttpClient client, ConfigureWriteFormData settings, Settings connectSettings)
        {
            Authenticator = new ApiAuthenticator(client, connectSettings);
            Client = client;
            Settings = settings;
            ConnectSettings = connectSettings;
            
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public ApiClient(HttpClient client, Settings connectSettings)
        {
            Authenticator = new ApiAuthenticator(client, connectSettings);
            Client = client;
            Settings = new ConfigureWriteFormData();
            ConnectSettings = connectSettings;
            
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
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

        public async Task<string> GetToken()
        {
            return await Authenticator.GetToken();
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

                if (Settings.Headers != null)
                {
                    foreach (var header in Settings.Headers)
                    {
                        if (header.Value.Contains(Constants.OktaTokenFind))
                        {
                            request.Headers.Add(header.Key, header.Value.Replace(Constants.OktaTokenFind, token));
                        }
                        else
                        {
                            request.Headers.Add(header.Key, header.Value);
                        }
                    }
                }

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
                
                if (Settings.Headers != null)
                {
                    foreach (var header in Settings.Headers)
                    {
                        if (header.Value.Contains(Constants.OktaTokenFind))
                        {
                            request.Headers.Add(header.Key, header.Value.Replace(Constants.OktaTokenFind, token));
                        }
                        else
                        {
                            request.Headers.Add(header.Key, header.Value);
                        }
                    }
                }
                
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
                
                if (Settings.Headers != null)
                {
                    foreach (var header in Settings.Headers)
                    {
                        if (header.Value.Contains(Constants.OktaTokenFind))
                        {
                            request.Headers.Add(header.Key, header.Value.Replace(Constants.OktaTokenFind, token));
                        }
                        else
                        {
                            request.Headers.Add(header.Key, header.Value);
                        }
                    }
                }

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
                
                if (Settings.Headers != null)
                {
                    foreach (var header in Settings.Headers)
                    {
                        if (header.Value.Contains(Constants.OktaTokenFind))
                        {
                            request.Headers.Add(header.Key, header.Value.Replace(Constants.OktaTokenFind, token));
                        }
                        else
                        {
                            request.Headers.Add(header.Key, header.Value);
                        }
                    }
                }

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
                
                if (Settings.Headers != null)
                {
                    foreach (var header in Settings.Headers)
                    {
                        if (header.Value.Contains(Constants.OktaTokenFind))
                        {
                            request.Headers.Add(header.Key, header.Value.Replace(Constants.OktaTokenFind, token));
                        }
                        else
                        {
                            request.Headers.Add(header.Key, header.Value);
                        }
                    }
                }

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