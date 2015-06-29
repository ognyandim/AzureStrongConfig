using System.Collections.Generic;

namespace Configuration.Interfaces
{
    public interface IServiceConfigurationProvider
    {
        Dictionary<string, Dictionary<string, string>> GetConfig();
    }
}