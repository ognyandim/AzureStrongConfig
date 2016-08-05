using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using Castle.Components.DictionaryAdapter;
using Castle.Core;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel;
using Castle.MicroKernel.Context;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers;
using Castle.Windsor;
using Configuration;
using Configuration.Interfaces;
using Persistence;
using Persistence.Interfaces;
using Worker.Services;
using Worker.Services.Interfaces;

namespace Worker
{
    public class WorkerContainerRegistrar
    {
        internal static WindsorContainer Container { get; set; }

        //private static readonly Type _implicitDependency = typeof(CloudConfigurationManager);

        public static void SetupContainer()
        {
            //Assembly.Load(_implicitDependency.Assembly.FullName);

            var container = new WindsorContainer();
            container.AddFacility<TypedFactoryFacility>();

            container.Register(Component.For<ILazyComponentLoader>().ImplementedBy<LazyOfTComponentLoader>());


            container.Register(Component.For<IMyWorkerService>().ImplementedBy<MyWorkerService>());
            container.Register(Component.For<IPersitenceServiceConfigDependent>().ImplementedBy<PersitenceServiceConfigDependent>());

            container.Register(Component.For<IAzureServiceConfigurationProvider>().ImplementedBy<DefaultAzureServiceConfigurationProvider>());

            container.Register(Component.For<IMicrosoftStorageConfig>().UsingFactoryMethod(GetAzureServiceConfig),
                Component.For<IAzureServiceConfiguration>().UsingFactoryMethod(GetAzureServiceConfiguration),
                Component.For<IConnectionStrings>().UsingFactoryMethod(GetConnectionStrings),
                Component.For<IAppConfigSettings>().UsingFactoryMethod(GetWebConfiuration));

            Container = container;
        }

        private static IMicrosoftStorageConfig GetAzureServiceConfig(IKernel k, ComponentModel cm, CreationContext cc)
        {
            var serviceConfig = new DefaultAzureServiceConfigurationProvider(k.Resolve<IAppConfigSettings>()).GetConfigRaw();

            Dictionary<string, string> workerConfig = null;
            if (!serviceConfig.TryGetValue("Worker", out workerConfig))
            {
                workerConfig = new Dictionary<string, string>();
            }

            return new DictionaryAdapterFactory().GetAdapter<IMicrosoftStorageConfig>(workerConfig);

        }

        private static IConnectionStrings GetConnectionStrings(IKernel k, ComponentModel cm, CreationContext cc)
        {
            var connectionStrings = ConfigurationManager.ConnectionStrings;
            Hashtable configHashtable = new Hashtable();
            foreach (var connectionString in connectionStrings)
            {
                var connString = (ConnectionStringSettings)connectionString;
                configHashtable.Add(connString.Name, connString.ConnectionString);
            }
            IConnectionStrings connStrings = new DictionaryAdapterFactory().GetAdapter<IConnectionStrings>(configHashtable);
            return connStrings;
        }

        private static IAppConfigSettings GetWebConfiuration(IKernel k, ComponentModel cm, CreationContext cc)
        {
            var webConfig = new DictionaryAdapterFactory().GetAdapter<IAppConfigSettings>(ConfigurationManager.AppSettings);
            return webConfig;
        }

        private static IAzureServiceConfiguration GetAzureServiceConfiguration(IKernel k, ComponentModel cm, CreationContext cc)
        {
            var rawConfigurationProvider = k.Resolve<IAzureServiceConfigurationProvider>();
            var azureServiceConfiguration = rawConfigurationProvider.GetConfig("Worker");
            return azureServiceConfiguration;
        }
    }
}