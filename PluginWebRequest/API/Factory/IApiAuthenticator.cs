using System.Threading.Tasks;

namespace PluginWebRequest.API.Factory
{
    public interface IApiAuthenticator
    {
        Task<string> GetToken();
    }
}