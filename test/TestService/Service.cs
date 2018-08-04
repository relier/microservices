using Microsoft.Extensions.Logging;

namespace Relier.Microservices.Tests.TestService
{
    public class Service : MicroserviceBase
    {
        public override void OnStart()
        {
            this.Logger.LogInformation("Starting TestService...");
            this.Logger.LogDebug("This is a debug log test.");
        }

        public override void Execute()
        {
            this.Logger.LogInformation("The Test Service is running...");

        }

        public override void OnStop()
        {
            this.Logger.LogInformation("Stopping TestService...");
        }
    }
}