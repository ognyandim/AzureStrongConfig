using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core;
using Castle.MicroKernel;
using Castle.MicroKernel.Context;
using Castle.MicroKernel.Lifestyle;
using IDependencyResolver = System.Web.Mvc.IDependencyResolver;

namespace CompositionRoot
{
    public class WindsorDependencyResolver : IDependencyResolver
    {
        private readonly IKernel _kernel;

        public WindsorDependencyResolver(IKernel kernel)
        {
            _kernel = kernel;
        }

        public object GetService(Type serviceType)
        {
            var handler = _kernel.GetHandler(serviceType);
            if (handler != null && CheckComponentLifeStyleSafe(handler))
            {
                return _kernel.Resolve(serviceType);
            }
            return null;
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            var handlers = _kernel.GetHandlers(serviceType);
            if (handlers != null && handlers.All(CheckComponentLifeStyleSafe))
            {
                return _kernel.ResolveAll(serviceType).Cast<object>();
            }
            return null;
        }

        private bool CheckComponentLifeStyleSafe(IHandler handler)
        {
            var lifestyleType = handler.ComponentModel.LifestyleType;
            if (typeof (IDisposable).IsAssignableFrom(handler.ComponentModel.Implementation))
            {
                return lifestyleType == LifestyleType.Undefined || //NOTE: Undefined is same as Singleton ???
                       lifestyleType == LifestyleType.Singleton ||
                       lifestyleType == LifestyleType.PerWebRequest ||
                       lifestyleType == LifestyleType.Scoped ||
                       (lifestyleType == LifestyleType.Custom &&
                        handler.ComponentModel.CustomLifestyle == typeof (TransientNonTrackedLifestyle));
            }
            return true;
        }

        private class TransientNonTrackedLifestyle : AbstractLifestyleManager
        {
            protected override Burden CreateInstance(CreationContext context, bool trackedExternally)
            {
                return base.CreateInstance(context, true);
            }

            public override void Dispose()
            {
            }
        }
    }
}