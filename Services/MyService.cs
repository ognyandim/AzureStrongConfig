using Configuration.Interfaces;
using Persistence.Interfaces;
using Services.Interfaces;

namespace Services
{
    public class MyService : IMyService
    {
        private readonly IConnectionStrings _connectionStrings;
        private readonly IAzureServiceConfiguration _azureServiceConfiguration;
        private readonly IMicrosoftStorageConfig _microsoftStorageConfig;
        private readonly IPersitenceServiceConfigDependent _persitenceServiceConfigDependent;

        public MyService(
            IConnectionStrings connectionStrings,
            IAzureServiceConfiguration azureServiceConfiguration,
            IMicrosoftStorageConfig microsoftStorageConfig,
            IPersitenceServiceConfigDependent persitenceServiceConfigDependent)
        {
            _connectionStrings = connectionStrings;
            _azureServiceConfiguration = azureServiceConfiguration;
            _microsoftStorageConfig = microsoftStorageConfig;
            _persitenceServiceConfigDependent = persitenceServiceConfigDependent;
        }

        public string DoWork()
        {
            _persitenceServiceConfigDependent.ConfigDependentAction("none");
            var configSetting = _microsoftStorageConfig.BlobConnectionstring;
            return "Job done :" + configSetting;
        }
    }
}