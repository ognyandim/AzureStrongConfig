using Configuration.Interfaces;
using Persistence.Interfaces;
using Services.Interfaces;

namespace Services
{
    public class MyService : IMyService
    {
        private readonly IMicrosoftStorageConfig _microsoftStorageConfig;
        private readonly IPersitenceServiceConfigDependent _persitenceServiceConfigDependent;

        public MyService(IMicrosoftStorageConfig microsoftStorageConfig,
            IPersitenceServiceConfigDependent persitenceServiceConfigDependent)
        {
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