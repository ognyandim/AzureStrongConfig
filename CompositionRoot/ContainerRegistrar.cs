using System;
using System.Collections;
using System.Configuration;
using System.Reflection;
using System.Web.Mvc;
using Castle.Components.DictionaryAdapter;
using Castle.Core;
using Castle.Core.Internal;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel;
using Castle.MicroKernel.Context;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers;
using Castle.Windsor;
using Configuration;
using Configuration.Interfaces;
using Microsoft.Azure;
using Persistence;
using Persistence.Interfaces;
using Services;
using Services.Interfaces;

namespace CompositionRoot
{
    public class ContainerRegistrar
    {
        internal static WindsorContainer Container { get; set; }

        private static readonly Type _implicitDependency = typeof(CloudConfigurationManager);

        public static void SetupContainer()
        {
            Assembly.Load(_implicitDependency.Assembly.FullName);

            var container = new WindsorContainer();
            container.AddFacility<TypedFactoryFacility>();

            container.Register(Component.For<ILazyComponentLoader>().ImplementedBy<LazyOfTComponentLoader>());

            container.Register(Component.For<IControllerFactory>().ImplementedBy<WindsorControllerFactory>());

            container.Register(Classes.FromAssembly(ReflectionUtil.GetAssemblyNamed("Web")).BasedOn<IController>().LifestylePerWebRequest());

            container.Register(Component.For<IMyService>().ImplementedBy<MyService>());
            container.Register(Component.For<IPersitenceServiceConfigDependent>().ImplementedBy<PersitenceServiceConfigDependent>());

            container.Register(Component.For<IAzureServiceConfigurationProvider>().ImplementedBy<DefaultAzureServiceConfigurationProvider>());

            container.Register(Component.For<IMicrosoftStorageConfig>().UsingFactoryMethod(GetAzureServiceConfig),
                Component.For<IAzureServiceConfiguration>().UsingFactoryMethod(GetAzureServiceConfiguration),
                Component.For<IConnectionStrings>().UsingFactoryMethod(GetConnectionStrings),
                Component.For<IWebConfigSettings>().UsingFactoryMethod(GetWebConfiuration));

            DependencyResolver.SetResolver(new WindsorDependencyResolver(container.Kernel));
            Container = container;
        }

        private static IMicrosoftStorageConfig GetAzureServiceConfig(IKernel k, ComponentModel cm, CreationContext cc)
        {
            var serviceConfig = new DefaultAzureServiceConfigurationProvider(k.Resolve<IWebConfigSettings>()).GetConfigRaw();
            var webConfig = serviceConfig["Web"];

            return new DictionaryAdapterFactory().GetAdapter<IMicrosoftStorageConfig>(webConfig);
        }

        private static IConnectionStrings GetConnectionStrings(IKernel k, ComponentModel cm, CreationContext cc)
        {
            var connectionStrings = ConfigurationManager.ConnectionStrings;
            Hashtable configHashtable = new Hashtable();
            foreach(var connectionString in connectionStrings)
            {
                var connString = (ConnectionStringSettings)connectionString;
                configHashtable.Add(connString.Name, connString.ConnectionString);
            }
            IConnectionStrings connStrings = new DictionaryAdapterFactory().GetAdapter<IConnectionStrings>(configHashtable);
            return connStrings;
        }

        private static IWebConfigSettings GetWebConfiuration(IKernel k, ComponentModel cm, CreationContext cc)
        {
            var webConfig = new DictionaryAdapterFactory().GetAdapter<IWebConfigSettings>(ConfigurationManager.AppSettings);
            return webConfig;
        }

        private static IAzureServiceConfiguration GetAzureServiceConfiguration(IKernel k, ComponentModel cm, CreationContext cc)
        {
            var rawConfigurationProvider = k.Resolve<IAzureServiceConfigurationProvider>();
            return rawConfigurationProvider.GetConfig();
        }
    }
}