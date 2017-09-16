using System;
using Ninject;
using NLog;

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
                Log.Info("Starting the service...");
                _kernel = NinjectConfig.Kernel;
                _arbitrager = _kernel.Get<IArbitrager>();
                _arbitrager.Start();
                Log.Info("Successfully started the service.");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                throw;
            }
        }

        public void Stop()
        {
            try
            {
                Log.Info("Stopping the service...");
                _arbitrager?.Dispose();
                _kernel?.Dispose();
                Log.Info("Successfully stopped the service.");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                throw;
            }
        }
    }
}