using Ninject;
using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WcfPad.Analysis;
using WcfPad.Analysis.Helpers;
using WcfPad.Invocation;

namespace WcfPad.UI
{
    public class DependencyInjection : NinjectModule
    {
        public static T Get<T>()
        {
            var kernel = new StandardKernel(new DependencyInjection());
            var instance = kernel.Get<T>();
            return instance;
        }

        public override void Load()
        {            
            // helpers
            Bind<IConfigHelper>().To<ConfigHelper>();
            Bind<IParameterTypeFactory>().To<ParameterTypeFactory>();
            Bind<IPathHelper>().To<PathHelper>();
            Bind<IWcfClientBuilder>().To<WcfClientBuilder>();
            Bind<IEndpointCache>().To<EndpointCache>();
            Bind<IEndpointDefinitionFactory>().To<EndpointDefinitionFactory>();
            Bind<IEndpointFactory>().To<EndpointFactory>();

            // services
            Bind<IAnalysisService>().To<AnalysisService>();
            Bind<IInvocationService>().To<InvocationService>();
        }
    }
}
