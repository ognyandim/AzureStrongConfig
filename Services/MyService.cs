﻿using Configuration.Interfaces;
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
        private readonly IAppConfigSettings _appConfigSettings;

        public MyService(
            IPersitenceServiceConfigDependent persitenceServiceConfigDependent,

            IConnectionStrings connectionStrings,
            IAzureServiceConfiguration azureServiceConfiguration,
            IMicrosoftStorageConfig microsoftStorageConfig,
            IAppConfigSettings appConfigSettings)
        {
            _connectionStrings = connectionStrings;
            _azureServiceConfiguration = azureServiceConfiguration;
            _microsoftStorageConfig = microsoftStorageConfig;
            _persitenceServiceConfigDependent = persitenceServiceConfigDependent;
            _appConfigSettings = appConfigSettings;
        }

        public string DoWork()
        {
            _persitenceServiceConfigDependent.ConfigDependentAction("none");
            var configSetting = _microsoftStorageConfig.StorageConnectionString;
            return $"Job done :" +
                   $"-- msConfig : {configSetting}, " +
                   $"-- azureConfig.ServiceBusConnectionString:{_azureServiceConfiguration.ServiceBusConnectionString} " +
                   $"-- webConfig.SubscriptionId:{_appConfigSettings.SubscriptionId} " +
                   $"-- connectionStrings.DefaultConnection :{_connectionStrings.DefaultConnection}";
        }
    }
}