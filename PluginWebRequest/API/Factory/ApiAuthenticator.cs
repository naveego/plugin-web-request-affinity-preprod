using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Naveego.Sdk.Logging;
using Newtonsoft.Json;
using PluginWebRequest.DataContracts;
using PluginWebRequest.Helper;

namespace PluginWebRequest.API.Factory
{
    public class ApiAuthenticator: IApiAuthenticator
    {
        private HttpClient Client { get; set; }
        private ConfigureWriteFormData Settings { get; set; }
        private string Token { get; set; }
        private DateTime ExpiresAt { get; set; }
        
        private const string AuthUrl = "";
        
        public ApiAuthenticator(HttpClient client, ConfigureWriteFormData settings)
        {
            Client = client;
            Settings = settings;
            ExpiresAt = DateTime.Now;
            Token = "";
        }

        public async Task<string> GetToken()
        {
            return await GetNewToken();
        }

        private async Task<string> GetNewToken()
        {
            return "";
        }
    }
}