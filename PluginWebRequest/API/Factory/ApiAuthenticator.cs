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
        private Settings ConnectSettings { get; set; }
        private string Token { get; set; }
        private DateTime ExpiresAt { get; set; }
        
        private const string AuthUrl = "https://aon-asc.oktapreview.com/oauth2/aus11pjxv9gRKnNDV0h8/v1/token";
        
        public ApiAuthenticator(HttpClient client, Settings connectSettings)
        {
            Client = client;
            ConnectSettings = connectSettings;
            ExpiresAt = DateTime.Now;
            Token = "";
        }

        public async Task<string> GetToken()
        {
            // check if token is expired or will expire in 5 minutes or less
            if (DateTime.Compare(DateTime.Now.AddMinutes(5), ExpiresAt) >= 0)
            {
                return await GetNewToken();
            }

            return Token;
        }

        private async Task<string> GetNewToken()
        {
            var formData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("client_id", ConnectSettings.ClientId),
                new KeyValuePair<string, string>("client_secret", ConnectSettings.ClientSecret),
                new KeyValuePair<string, string>("refresh_token", ConnectSettings.RefreshToken)
            };
            
            var body = new FormUrlEncodedContent(formData);

            var response = await Client.PostAsync(AuthUrl, body);
            response.EnsureSuccessStatusCode();

            var content = JsonConvert.DeserializeObject<TokenResponse>(await response.Content.ReadAsStringAsync());
            ExpiresAt = DateTime.Now.AddSeconds(content.ExpiresIn);
            Token = content.AccessToken;
            
            return Token;
        }
    }
}