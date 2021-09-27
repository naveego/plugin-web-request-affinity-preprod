using PluginWebRequestAffinityPreprod.DataContracts;
using PluginWebRequestAffinityPreprod.Helper;

namespace PluginWebRequestAffinityPreprod.API.Factory
{
    public interface IApiClientFactory
    {
        IApiClient CreateApiClient(ConfigureWriteFormData settings, Settings connectSettings);
        IApiClient CreateApiClient(Settings connectSettings);
    }
}