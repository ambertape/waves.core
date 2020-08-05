﻿using System;
using System.Collections.Generic;

namespace Waves.Core.Base.Interfaces
{
    /// <summary>
    /// Event args for application actions updating.
    /// </summary>
    public class ApplicationActionsUpdatedEventArgs
    {
        /// <summary>
        /// Creates new instance of <see cref="ApplicationActionsUpdatedEventArgs"/>.
        /// </summary>
        /// <param name="removedActions">Removed actions collection.</param>
        /// <param name="addedActions">Added actions collection.</param>
        public ApplicationActionsUpdatedEventArgs(ICollection<IApplicationAction> removedActions, ICollection<IApplicationAction> addedActions)
        {
            RemovedActions = removedActions;
            AddedActions = addedActions;
        }

        /// <summary>
        /// Gets removed actions collection.
        /// </summary>
        public ICollection<IApplicationAction> RemovedActions { get; }

        /// <summary>
        /// Gets added actions collection.
        /// </summary>
        public ICollection<IApplicationAction> AddedActions { get; }
    }

    /// <summary>
    /// Delegate for applications actions update handling.
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <param name="args">Arguments.</param>
    public delegate void ApplicationsActionsUpdatedEventHandler(object sender, ApplicationActionsUpdatedEventArgs args);

    /// <summary>
    ///     Interface of application classes.
    /// </summary>
    public interface IApplication : IObject, IDisposable
    {
        /// <summary>
        ///     Gets whether the application is initialized.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        ///     Gets icon's background color.
        /// </summary>
        IColor IconBackgroundColor { get; }

        /// <summary>
        ///     Gets icon's foreground color.
        /// </summary>
        IColor IconForegroundColor { get; }

        /// <summary>
        ///     Gets application's configuration.
        /// </summary>
        IConfiguration Configuration { get; }

        /// <summary>
        ///     Gets application's icon.
        /// </summary>
        string Icon { get; }

        /// <summary>
        ///     Gets description.
        /// </summary>
        string Description { get; }

        /// <summary>
        ///     Gets manufacturer.
        /// </summary>
        string Manufacturer { get; }

        /// <summary>
        ///     Gets version.
        /// </summary>
        Version Version { get; }

        /// <summary>
        ///     Gets collection of available actions.
        /// </summary>
        ICollection<IApplicationAction> Actions { get; }

        /// <summary>
        ///     Event for actions updated.
        /// </summary>
        event ApplicationsActionsUpdatedEventHandler ActionsUpdated;

        /// <summary>
        ///     Initializes application.
        /// </summary>
        void Initialize();

        /// <summary>
        ///     Saves configuration.
        /// </summary>
        void SaveConfiguration();
    }
}