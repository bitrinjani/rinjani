using System.Reflection;
using Topshelf;

namespace Rinjani
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var serviceName = Assembly.GetExecutingAssembly().GetName().Name;
            HostFactory.Run(
                hostConfig =>
                {
                    hostConfig.Service<AppRoot>(
                        serviceConfig =>
                        {
                            serviceConfig.ConstructUsing(name => new AppRoot());
                            serviceConfig.WhenStarted(service => service.Start());
                            serviceConfig.WhenStopped(service => service.Stop());
                        });
                    hostConfig.RunAsLocalSystem();
                    hostConfig.SetDescription(serviceName);
                    hostConfig.SetDisplayName(serviceName);
                    hostConfig.SetServiceName(serviceName);
                });
        }
    }
}