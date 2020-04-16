using System;
using System.Collections.Generic;
using System.Composition;
using Fluid.Core.Base;
using Fluid.Core.Base.Enums;
using Fluid.Core.Base.Interfaces;
using Fluid.Core.Services.Interfaces;
using NLog;
using NLog.Common;

namespace Fluid.Core.Services.Logging
{
    /// <summary>
    ///     Logging service.
    /// </summary>
    [Export(typeof(IService))]
    public class Service : Services.Service, ILoggingService
    {
        private readonly string _currentDirectory = Environment.CurrentDirectory;
        private Logger _logger;

        /// <inheritdoc />
        public override Guid Id => Guid.Parse("D17B3463-C126-4023-B22F-1A031636A343");

        /// <inheritdoc />
        public override string Name { get; set; } = "Logging Service";

        /// <inheritdoc />
        public int LastMessagesCount { get; private set; } = 250;

        /// <inheritdoc />
        public ICollection<IMessage> LastMessages { get; } = new List<IMessage>();

        /// <inheritdoc />
        public override void Initialize()
        {
            if (IsInitialized) return;

            IsInitialized = true;

            _logger = LogManager.GetCurrentClassLogger();

            InternalLogger.LogToConsole = false;

            OnMessageReceived(this,
                new Message("Initialization", "Service was initialized.", Name, MessageType.Information));
        }

        /// <inheritdoc />
        public override void LoadConfiguration(IConfiguration configuration)
        {
            LastMessagesCount = LoadConfigurationValue(configuration, "LoggingService-LastMessagesCount", 250);
        }

        /// <inheritdoc />
        public override void SaveConfiguration(IConfiguration configuration)
        {
            configuration.SetPropertyValue("LoggingService-LastMessagesCount", LastMessagesCount);
        }

        /// <inheritdoc />
        public override void Dispose()
        {
        }

        /// <inheritdoc />
        public void WriteTextToLog(string text)
        {
            if (!IsInitialized) return;

            var message = new Message(string.Empty, text, string.Empty, MessageType.Information);

            AddMessageToCollection(message);

            _logger.Info("{0} {1}",
                DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString(),
                text);
        }

        /// <inheritdoc />
        public void WriteMessageToLog(IMessage message)
        {
            if (!IsInitialized) return;

            AddMessageToCollection(message);

            switch (message.Type)
            {
                case MessageType.Information:
                    _logger.Info("{0} {1} {2}: {3} - {4}",
                        message.DateTime.ToShortDateString() + " " + message.DateTime.ToShortTimeString(),
                        "[INFORMATION]",
                        message.Sender, message.Title, message.Text);
                    break;
                case MessageType.Warning:
                    _logger.Warn("{0} {1} {2}: {3} - {4}",
                        message.DateTime.ToShortDateString() + " " + message.DateTime.ToShortTimeString(),
                        "[WARNING]",
                        message.Sender, message.Title, message.Text);
                    break;
                case MessageType.Error:
                    _logger.Error("{0} {1} {2}: {3} - {4}",
                        message.DateTime.ToShortDateString() + " " + message.DateTime.ToShortTimeString(),
                        "[ERROR]",
                        message.Sender, message.Title, message.Text);
                    break;
                case MessageType.Success:
                    _logger.Info("{0} {1} {2}: {3} - {4}",
                        message.DateTime.ToShortDateString() + " " + message.DateTime.ToShortTimeString(),
                        "[SUCCESS]",
                        message.Sender, message.Title, message.Text);
                    break;
                case MessageType.Debug:
                    _logger.Debug("{0} {1} {2}: {3} - {4}",
                        message.DateTime.ToShortDateString() + " " + message.DateTime.ToShortTimeString(),
                        "[DEBUG]",
                        message.Sender, message.Title, message.Text);
                    break;
                case MessageType.Fatal:
                    _logger.Fatal("{0} {1} {2}: {3} - {4}",
                        message.DateTime.ToShortDateString() + " " + message.DateTime.ToShortTimeString(),
                        "[FATAL]",
                        message.Sender, message.Title, message.Text);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <inheritdoc />
        public void WriteExceptionToLog(Exception exception, string sender, bool isFatal)
        {
            if (!IsInitialized) return;

            var message = new Message(exception.Source, exception.Message, sender, MessageType.Error);

            AddMessageToCollection(message);

            if (isFatal)
                _logger.Fatal("{0} {1} {2}: {3} - {4}",
                    message.DateTime.ToShortDateString() + " " + message.DateTime.ToShortTimeString(),
                    "[FATAL]",
                    message.Sender, message.Title, message.Text);
            else
                _logger.Error("{0} {1} {2}: {3} - {4}",
                    message.DateTime.ToShortDateString() + " " + message.DateTime.ToShortTimeString(),
                    "[ERROR]",
                    message.Sender, message.Title, message.Text);
        }

        /// <summary>
        ///     Adds message to collection.
        /// </summary>
        /// <param name="message">Message.</param>
        private void AddMessageToCollection(IMessage message)
        {
            LastMessages.Add(message);

            if (LastMessages.Count > LastMessagesCount)
                (LastMessages as List<IMessage>)?.RemoveAt(0);
        }
    }
}