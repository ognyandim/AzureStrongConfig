using Configuration.Interfaces;
using Persistence.Interfaces;

namespace Persistence
{
    public class PersitenceServiceConfigDependent : IPersitenceServiceConfigDependent
    {
        private readonly IMicrosoftStorageConfig _microsoftStorageConfig;

        public PersitenceServiceConfigDependent(
            IMicrosoftStorageConfig microsoftStorageConfig)
        {
            _microsoftStorageConfig = microsoftStorageConfig;
        }

        public void ConfigDependentAction(string argument)
        {
            var configSetting = _microsoftStorageConfig.BlobConnectionstring;
        }
    }
}