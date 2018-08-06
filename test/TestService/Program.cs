using System;

namespace Relier.Microservices.Tests.TestService
{
    class Program
    {
        static void Main(string[] args)
        {
            var service = new Service();
            service.KeepRunning(5000);
        }
    }
}
