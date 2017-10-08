using System;
using Ninject;
using NLog;
using System.Configuration;
using System.Globalization;
using Rinjani.Properties;

namespace Rinjani
{
    internal class AppRoot
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private IArbitrager _arbitrager;
        private IKernel _kernel;

        public void Start()
        {
            try
            {
                var culture = ConfigurationManager.AppSettings["Culture"];
                if (culture != null)
                {
                    CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(culture);
                }
                Log.Info(Resources.StartingTheService);
                _kernel = NinjectConfig.Kernel;
                _arbitrager = _kernel.Get<IArbitrager>();
                _arbitrager.Start();
                Log.Info(Resources.SuccessfullyStartedTheService);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                if (Environment.UserInteractive)
                {
                    Console.ReadLine();
                }
                throw;
            }
        }

        public void Stop()
        {
            try
            {
                Log.Info(Resources.StoppingTheService);
                _arbitrager?.Dispose();
                _kernel?.Dispose();
                Log.Info(Resources.SuccessfullyStoppedTheService);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                throw;
            }
        }
    }
}