﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fluid.Core.Base;
using Fluid.Core.Base.Enums;
using Fluid.Core.Base.Interfaces;
using Fluid.Core.IoC;
using Fluid.Core.Services;
using Fluid.Core.Services.Interfaces;
using Fluid.Utils.Serialization;

namespace Fluid.Core
{
    /// <summary>
    /// Core.
    /// </summary>
    public class Core
    {
        private ILoggingService _loggingService;
        
        /// <summary>
        ///     Gets or sets collection of services.
        /// </summary>
        private readonly ICollection<IService> _services = new List<IService>();
        
        /// <summary>
        ///     Gets or sets whether configuration initiliazed.
        /// </summary>
        public bool IsConfigurationInitialized { get; private set; }
        
        /// <summary>
        /// Gets or sets whetger logging initialized.
        /// </summary>
        public bool IsLoggingInitialized{ get; private set; }

        /// <summary>
        ///     Gets or sets configuration.
        /// </summary>
        public IConfiguration Configuration { get; private set; }

        /// <summary>
        ///     Starts core working.
        /// </summary>
        public void Start()
        {
            try
            {
                ContainerCore.Start();

                InitializeConfiguration();
                InitializeServices();

                WriteLogMessage(new Message("Core launching.", "Core launching successfully.", "Core",MessageType.Information));
            }
            catch (Exception e)
            {
                WriteLogMessage(new Message("Core launching.", "Error starting kernel:\r\n" + e, "Core", MessageType.Error));
            }
        }

        /// <summary>
        ///     Stops core working.
        /// </summary>
        public void Stop()
        {
            try
            {
                SaveConfiguration();
                StopServices();

                WriteLogMessage(new Message("Core stopping.", "Core stopping successfully.", "Core",MessageType.Information));
            }
            catch (Exception e)
            {
                WriteLogMessage(new Message("Core stopping.", "Error stopping kernel:\r\n" + e, "Core", MessageType.Error));
            }
        }

        /// <summary>
        ///     Saves configuration.
        /// </summary>
        public void SaveConfiguration()
        {
            try
            {
                CheckConfigurationDirectory();

                var fileName = Path.Combine(
                    Environment.CurrentDirectory,
                    "config",
                    "core.config");

                if (File.Exists(fileName))
                    File.Delete(fileName);

                Json.WriteToFile(fileName, Configuration);
            }
            catch (Exception e)
            {
                WriteLogMessage(new Message("Configuration saving.", "Error configuration saving:\r\n" + e, "Core", MessageType.Error));
            }
        }

        /// <summary>
        ///     Gets service by type.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <returns>Service.</returns>
        public T GetService<T>()
        {
            try
            {
                return (T) ContainerCore.GetInstance(typeof(T), null);
            }
            catch (Exception e)
            {
                WriteLogMessage(new Message("Getting service.", "Error getting service:\r\n" + e, "Core", MessageType.Error));
                
                return default;
            }
        }

        /// <summary>
        ///     Registers service.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="instance">Instance.</param>
        public void RegisterService<T>(T instance)
        {
            try
            {
                if (!(instance is IService service)) return;

                ContainerCore.RegisterService(instance);

                service.MessageReceived += OnServiceMessageReceived;

                service.Initialize();

                service.LoadConfiguration(Configuration);

                if (service.IsInitialized)
                    _services.Add(service);
            }
            catch (Exception e)
            {
                WriteLogMessage(new Message("Registering service.", "Error registering service:\r\n" + e, "Core", MessageType.Error));
            }
        }

        /// <summary>
        ///     Initializes core configuration.
        /// </summary>
        private void InitializeConfiguration()
        {
            try
            {
                CheckConfigurationDirectory();

                var fileName = Path.Combine(
                    Environment.CurrentDirectory,
                    "config",
                    "core.config");

                Configuration = File.Exists(fileName)
                    ? Json.ReadFile<Configuration>(fileName)
                    : new Configuration();

                IsConfigurationInitialized = true;
            }
            catch (Exception e)
            {
                WriteLogMessage(new Message("Configuration initialization.", "Error configuration initialization:\r\n" + e, "Core", MessageType.Error));
            }
        }

        /// <summary>
        ///     Initializes base core services.
        /// </summary>
        private void InitializeServices()
        {
            RegisterService(Manager.GetService<ILoggingService>().FirstOrDefault());

            InitializeLogging();
            
            RegisterService(Manager.GetService<IInputService>().FirstOrDefault());
            RegisterService(Manager.GetService<IModuleService>().FirstOrDefault());
            RegisterService(Manager.GetService<IApplicationService>().FirstOrDefault());
        }

        /// <summary>
        /// Initializes logging.
        /// </summary>
        private void InitializeLogging()
        {
            try
            {
                _loggingService = GetService<ILoggingService>();

                if (_loggingService != null)
                    IsLoggingInitialized = true;
            }
            catch (Exception)
            {
                IsLoggingInitialized = false;
            }
        }
        
        /// <summary>
        /// Stops services.
        /// </summary>
        private void StopServices()
        {
            foreach (var service in _services)
            {
                service.Dispose();
            }
        }

        /// <summary>
        ///     Notifies when service receive message.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="message">Message.</param>
        private void OnServiceMessageReceived(object sender, IMessage message)
        {
            WriteLogMessage(message);
        }

        /// <summary>
        ///     Writes text to log.
        /// </summary>
        /// <param name="text">Text.</param>
        public void WriteLog(string text)
        {
            if (!IsLoggingInitialized) return;
            
            _loggingService.WriteTextToLog(text);
        }

        /// <summary>
        ///     Writes message to log.
        /// </summary>
        /// <param name="message">Message..</param>
        public void WriteLogMessage(IMessage message)
        {
            if (!IsLoggingInitialized) return;
            
            _loggingService.WriteMessageToLog(message);
        }

        /// <summary>
        ///     Writes exception to log.
        /// </summary>
        /// <param name="exception">Exception.</param>
        /// <param name="sender">Sender.</param>
        public void WriteLogMessage(Exception exception, string sender)
        {
            if (!IsLoggingInitialized) return;
            
            _loggingService.WriteExceptionToLog(exception, sender);
        }

        /// <summary>
        ///     Checks configuration directory.
        /// </summary>
        private void CheckConfigurationDirectory()
        {
            var directoryName = Path.Combine(
                Environment.CurrentDirectory,
                "config");

            if (!Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);
        }
    }
}