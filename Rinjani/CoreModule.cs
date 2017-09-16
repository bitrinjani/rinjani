using Ninject;
using Ninject.Modules;
using RestSharp;
using Rinjani.Bitflyer;

namespace Rinjani
{
    public class CoreModule : NinjectModule
    {
        public override void Load()
        {
            var path = "config.json";
            Kernel.Bind<IConfigStore>().To<JsonConfigStore>().InSingletonScope()
                .WithConstructorArgument("path", path);
            Kernel.Bind<IQuoteAggregator>().To<QuoteAggregator>().InSingletonScope();
            Kernel.Bind<IPositionService>().To<PositionService>().InSingletonScope();
            Kernel.Bind<IBrokerAdapterRouter>().To<BrokerAdapterRouter>();
            Kernel.Bind<IArbitrager>().To<Arbitrager>();
            Kernel.Bind<ISpreadAnalyzer>().To<SpreadAnalyzer>();
            Kernel.Bind<IBrokerAdapter>().To<BrokerAdapter>();
            Kernel.Bind<IBrokerAdapter>().To<Coincheck.BrokerAdapter>();
            Kernel.Bind<IBrokerAdapter>().To<Quoine.BrokerAdapter>();
            Kernel.Bind<IRestClient>().To<RestClient>();
            Kernel.Bind<ITimer>().To<TimerAdapter>();
        }
    }

    public class NinjectConfig
    {
        private static IKernel _kernel;
        public static IKernel Kernel => _kernel ?? (_kernel = new StandardKernel(new CoreModule()));
    }
}