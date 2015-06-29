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
using Persistence;
using Persistence.Interfaces;
using Services;
using Services.Interfaces;

namespace CompositionRoot
{
    public class ContainerRegistrar
    {
        internal static WindsorContainer Container { get; set; }

        public static void SetupContainer()
        {
            var container = new WindsorContainer();
            container.AddFacility<TypedFactoryFacility>();

            container.Register(Component.For<ILazyComponentLoader>().ImplementedBy<LazyOfTComponentLoader>());

            container.Register(Component.For<IControllerFactory>().ImplementedBy<WindsorControllerFactory>());

            container.Register(Classes.FromAssembly(ReflectionUtil.GetAssemblyNamed("Web")).BasedOn<IController>().LifestylePerWebRequest());

            container.Register(Component.For<IMyService>().ImplementedBy<MyService>());
            container.Register(
                Component.For<IPersitenceServiceConfigDependent>().ImplementedBy<PersitenceServiceConfigDependent>());


            container.Register(Component.For<IMicrosoftStorageConfig>().UsingFactoryMethod(GetAzureServiceConfig));

            DependencyResolver.SetResolver(new WindsorDependencyResolver(container.Kernel));
            Container = container;
        }

        private static IMicrosoftStorageConfig GetAzureServiceConfig(IKernel arg1, ComponentModel arg2,
            CreationContext arg3)
        {
            var serviceConfig = new DefaultServiceConfigurationProvider().GetConfig();
            var webConfig = serviceConfig["Web"];

            return new DictionaryAdapterFactory().GetAdapter<IMicrosoftStorageConfig>(webConfig);
        }
    }
}