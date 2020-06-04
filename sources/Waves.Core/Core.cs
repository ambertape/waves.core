﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Waves.Core.Base;
using Waves.Core.Base.Enums;
using Waves.Core.Base.Interfaces;
using Waves.Core.IoC;
using Waves.Core.Services;
using Waves.Core.Services.Interfaces;
using Waves.Utils.Serialization;

namespace Waves.Core
{
    /// <summary>
    ///     Core.
    /// </summary>
    public class Core
    {
        private readonly List<IMessage> _pendingMessages = new List<IMessage>();

        private ILoggingService _loggingService;

        /// <summary>
        ///     Gets whether Core is running.
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        ///     Gets configuration.
        /// </summary>
        public IConfiguration Configuration { get; private set; }

        /// <summary>
        ///     Gets service manager.
        /// </summary>
        public Manager ServiceManager { get; } = new Manager();

        /// <summary>
        ///     Gets collections of registered services.
        /// </summary>
        public ICollection<IService> Services { get; } = new List<IService>();

        /// <summary>
        ///     Gets service initialization information dictionary.
        ///     Dictionary includes info about base service by default.
        /// </summary>
        public Dictionary<string, bool> CoreInitializationInformationDictionary { get; } = new Dictionary<string, bool>
        {
            {"Configuration Loader Service", false},
            {"Service Container", false},
            {"Application Loader Service", false},
            {"Keyboard and Mouse Input Service", false},
            {"Logging Service", false},
            {"Module Loader Service", false}
        };

        /// <summary>
        ///     Event for message receiving handling.
        /// </summary>
        public event EventHandler<IMessage> MessageReceived;

        /// <summary>
        ///     Starts core working.
        /// </summary>
        public virtual void Start()
        {
            try
            {
                WriteLogMessage(new Message("Core launch", "Core is launching...", "Core",
                    MessageType.Success));

                InitializeConfiguration();
                InitializeContainer();
                InitializeServices();

                IsRunning = true;

                WriteLogMessage(new Message("Core launch", "Core launching successfully.", "Core",
                    MessageType.Success));

                AddMessageSeparator();
            }
            catch (Exception e)
            {
                WriteLogMessage(new Message("Core launch", "Error starting kernel.", "Core", e, true));
            }
        }

        /// <summary>
        ///     Stops core working.
        /// </summary>
        public virtual void Stop()
        {
            try
            {
                SaveConfiguration();
                StopServices();

                ServiceManager.MessageReceived -= OnServiceMessageReceived;

                WriteLogMessage(new Message("Core stop", "Core stopped successfully.", "Core",
                    MessageType.Success));

                WriteLog("----------------------------------------------------");
            }
            catch (Exception e)
            {
                WriteLogMessage(new Message("Core stop", "Error stopping kernel.", "Core", e, true));
            }
        }

        /// <summary>
        ///     Saves configuration.
        /// </summary>
        public void SaveConfiguration()
        {
            try
            {
                foreach (var service in Services)
                    try
                    {
                        service.SaveConfiguration(Configuration);
                    }
                    catch (Exception)
                    {
                        throw new Exception("Error saving \"" + service.Name + "\" configuration.");
                    }

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
                WriteLogMessage(new Message("Saving configuration", "Error configuration saving:\r\n" + e, "Core",
                    MessageType.Error));
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
                WriteLogMessage(new Message("Getting service", "Error getting service.", "Core", e, true));

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
                
                if (CoreInitializationInformationDictionary.ContainsKey(service.Name))
                {
                    CoreInitializationInformationDictionary[service.Name] = service.IsInitialized;
                }
                else
                {
                    CoreInitializationInformationDictionary.Add(service.Name, service.IsInitialized);
                }
                
                if (service.IsInitialized)
                    Services.Add(service);
            }
            catch (Exception e)
            {
                WriteLogMessage(new Message("Registering service", "Error registering service.", "Core", e, true));

                if (!(instance is IService service)) return;

                CoreInitializationInformationDictionary[service.Name] = false;
            }
        }

        /// <summary>
        ///     Writes text to log.
        /// </summary>
        /// <param name="text">Text.</param>
        public virtual void WriteLog(string text)
        {
#if DEBUG
            Console.WriteLine(text);
#endif

            if (!CoreInitializationInformationDictionary["Logging Service"]) return;

            OnMessageReceived(new Message(string.Empty, text, string.Empty, MessageType.Information));

            CheckLoggingService();

            _loggingService.WriteTextToLog(text);
        }

        /// <summary>
        ///     Writes message to log.
        /// </summary>
        /// <param name="message">Message..</param>
        public virtual void WriteLogMessage(IMessage message)
        {
#if DEBUG
            Console.WriteLine("{0} {1}: {2}", message.DateTime, message.Sender, message.Title + " - " + message.Text);

            if (message.Exception != null)
            {
                Console.WriteLine(message.Exception.ToString());
            }
#endif

            OnMessageReceived(message);

            if (!CoreInitializationInformationDictionary["Logging Service"])
            {
                _pendingMessages.Add(message);

                return;
            }

            CheckLoggingService();

            _loggingService.WriteMessageToLog(message);
        }

        /// <summary>
        ///     Writes exception to log.
        /// </summary>
        /// <param name="exception">Exception.</param>
        /// <param name="sender">Sender.</param>
        /// <param name="isFatal">Sets whether exception is fatal.</param>
        public virtual void WriteLogException(Exception exception, string sender, bool isFatal)
        {
#if DEBUG
            Console.WriteLine("Core exception: {0}", exception);
#endif

            var message = new Message(exception, false);

            OnMessageReceived(message);

            if (!CoreInitializationInformationDictionary["Logging Service"])
            {
                _pendingMessages.Add(message);

                return;
            }

            CheckLoggingService();

            _loggingService.WriteExceptionToLog(exception, sender, isFatal);
        }

        /// <summary>
        ///     Adds message separator.
        /// </summary>
        public virtual void AddMessageSeparator()
        {
            _loggingService.LastMessages.Add(new MessageSeparator());
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

                Configuration.Initialize();

                CoreInitializationInformationDictionary["Configuration Loader Service"] = true;
            }
            catch (Exception e)
            {
                WriteLogMessage(new Message("Configuration initialization",
                    "Error configuration initialization", "Core", e, true));
            }
        }

        /// <summary>
        ///     Initializes container.
        /// </summary>
        private void InitializeContainer()
        {
            try
            {
                ContainerCore.Start();

                CoreInitializationInformationDictionary["Service Container"] = true;
            }
            catch (Exception e)
            {
                WriteLogMessage(new Message("Container initialization", "Error container initialization.", "Core", e,
                    true));

                CoreInitializationInformationDictionary["Service Container"] = false;
            }
        }

        /// <summary>
        ///     Initializes base core services.
        /// </summary>
        private void InitializeServices()
        {
            ServiceManager.MessageReceived += OnServiceMessageReceived;

            ServiceManager.Initialize();

            var method = typeof(Core).GetMethod("RegisterService");
            if (method == null) return;
            
            foreach (var service in ServiceManager.Services)
            {
                try
                {
                    var type = service.GetType();
                    var interfaces = type.GetInterfaces().ToList();
                    
                    interfaces.Remove(typeof(IObservableObject));
                    interfaces.Remove(typeof(INotifyPropertyChanged));
                    interfaces.Remove(typeof(IDisposable));
                    interfaces.Remove(typeof(IObject));
                    interfaces.Remove(typeof(IService));

                    if (interfaces.Count > 1)
                    {
                        throw new Exception("A service cannot be registered (a service must be implemented from only one interface).");
                    }
                    
                    var genericMethod = method.MakeGenericMethod(interfaces.First());
                    genericMethod.Invoke(this, new object[] {service});

                    WriteLogMessage(new Message("Registering", "Service has been registered successfully.", service.Name,
                        MessageType.Success));
                }
                catch (Exception e)
                {
                    WriteLogMessage(new Message("Registering",
                        "Error registering service.", service.Name, e, true));
                }
            }
        }

        /// <summary>
        ///     Stops services.
        /// </summary>
        private void StopServices()
        {
            foreach (var service in Services) service.Dispose();
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
        ///     Invokes message received event.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnMessageReceived(IMessage e)
        {
            MessageReceived?.Invoke(this, e);
        }

        private void CheckLoggingService()
        {
            if (_loggingService == null)
            {
                _loggingService = GetService<ILoggingService>();

                foreach (var message in _pendingMessages) _loggingService.WriteMessageToLog(message);

                _pendingMessages.Clear();
            }
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