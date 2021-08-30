using PluginWebRequest.DataContracts;
using PluginWebRequest.Helper;

namespace PluginWebRequest.API.Factory
{
    public interface IApiClientFactory
    {
        IApiClient CreateApiClient(ConfigureWriteFormData settings);
    }
}