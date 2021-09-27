using System.Net.Http;
using System.Threading.Tasks;

namespace PluginWebRequestAffinityPreprod.API.Factory
{
    public interface IApiClient
    {
        Task TestConnection();
        Task<string> GetToken();
        Task<HttpResponseMessage> GetAsync(string path);
        Task<HttpResponseMessage> PostAsync(string path, StringContent json);
        Task<HttpResponseMessage> PutAsync(string path, StringContent json);
        Task<HttpResponseMessage> PatchAsync(string path, StringContent json);
        Task<HttpResponseMessage> DeleteAsync(string path);
    }
}