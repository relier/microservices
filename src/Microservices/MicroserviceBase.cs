using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog.Config;
using NLog.Extensions.Logging;
using NLog.Targets;
using System;
using System.IO;
using System.Runtime.Loader;
using System.Threading;

namespace Relier.Microservices
{
    /// <summary>Base class to construct microservices.</summary>
    public abstract class MicroserviceBase
    {
        /// <summary>Instance of logger object.</summary>
        protected ILogger Logger { get; private set; }

        /// <summary>Control flag to stop this microservice.</summary>
        private bool _stopCommand = false;

        /// <summary>Control flag that indicates that this service can gracefully stop.</summary>
        private bool _stopConfirmation = false;

        /// <summary>Executation interval between each service iteration.</summary>
        private int _executionInterval = 1000;

        /// <summary>Basic constructor.</summary>
        public MicroserviceBase()
        {
            // Listen the ProcessExit event to control service stop.
            AppDomain.CurrentDomain.ProcessExit += (s, e) => StopEvent(s, e);

            // Log configuration.
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddNLog();
            LoggingConfiguration logConfig = new LoggingConfiguration();

            string instanceId = System.Environment.MachineName.Substring(0, 12);
            
            // Read appsettings.json configuration file.
            string configPath = "appsettings.json";
            var file = new FileInfo(configPath);
            string env = GetEnvironmentConfigFileSuffix();
            string fileName = file.Name.Substring(0, file.Name.Length - file.Extension.Length);
            string envConfigPath = fileName + "." + env + file.Extension;
            
            var builder = new ConfigurationBuilder()
                .AddJsonFile(configPath, true)
                .AddJsonFile(envConfigPath, true);

            var config = builder.Build();
            var section = config.GetSection("Relier.Microservice");
            if (section.Exists())
            {
                if (section.GetValue<bool>("Debug"))
                {
                    FileTarget debugFileTarget = new FileTarget();
                    debugFileTarget.FileName = "${basedir}/logs/log-" + instanceId + "-debug.log";
                    debugFileTarget.Layout = "${date} ${level:uppercase=true} ${message} ${exception}";            
                    logConfig.AddTarget("debugFileLog", debugFileTarget);
                    logConfig.AddRule(NLog.LogLevel.Debug, NLog.LogLevel.Fatal, "debugFileLog");
                }
            }

            FileTarget generalFileLog = new FileTarget();
            generalFileLog.FileName = "${basedir}/logs/log-" + instanceId + ".log";
            generalFileLog.Layout = "${date} ${level:uppercase=true} ${message} ${exception}";
            logConfig.AddTarget("generalFileLog", generalFileLog);
            logConfig.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, "generalFileLog");

            NLog.LogManager.Configuration = logConfig;
            this.Logger = loggerFactory.CreateLogger<MicroserviceBase>();
        }

        /// <summary>Start the execution of this microservice.</summary>
        /// <param name="executionInterval">Execution interval between each microservice iteration.</param>
        public void KeepRunning(int executionInterval)
        {
            _executionInterval = executionInterval;
            StartEvent();
        }

        /// <summary>Controls the execution of this microservice.</summary>
        private void StartEvent()
        {
            OnStart();
            
            while(!_stopCommand)
            {
                Execute();
                Thread.Sleep(_executionInterval);
            }

            _stopConfirmation = true;

            OnStop();
        }

        /// <summary>Called when this microservice is starting.</summary>
        public abstract void OnStart();

        /// <summary>Called on each iteration.</summary>
        public abstract void Execute();

        /// <summary>Called when this microservice is stopping.</summary>
        public abstract void OnStop();

        /// <summary>Handler to stop gracefully stop this microservice.</summary>
        private void StopEvent(object obj, EventArgs e)
        {
            _stopCommand = true;

            while(!_stopConfirmation)
                Thread.Sleep(500);
        }

        /// <summmary>Identify environment and return the configuration file suffix.</summary>
        /// <returns>File suffix.</returns>
        private string GetEnvironmentConfigFileSuffix()
        {
            string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (string.IsNullOrEmpty(env))
                return "Production";

            return env;
        }
    }
}
