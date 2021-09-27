using System.Threading.Tasks;

namespace PluginWebRequestAffinityPreprod.API.Factory
{
    public interface IApiAuthenticator
    {
        Task<string> GetToken();
    }
}