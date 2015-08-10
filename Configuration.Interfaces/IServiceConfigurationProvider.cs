using System.Collections.Generic;

namespace Configuration.Interfaces
{
    public interface IAzureServiceConfigurationProvider
    {
        Dictionary<string, Dictionary<string, string>> GetConfigRaw();
        IAzureServiceConfiguration GetConfig();
    }
}