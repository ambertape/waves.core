using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using Fluid.Core.Base;
using Fluid.Core.Base.Enums;
using Fluid.Core.Base.Interfaces;
using Fluid.Core.Services.Interfaces;

namespace Fluid.Core.Services
{
    /// <summary>
    /// Service manager.
    /// </summary>
    public static class Manager
    {
        private static readonly string CurrentDirectory = Environment.CurrentDirectory;

        /// <summary>
        /// Event for message receiving handling.
        /// </summary>
        public static event EventHandler<IMessage> MessageReceived; 

        /// <summary>
        /// Gets or sets collection of services.
        /// </summary>
        [ImportMany]
        public static IEnumerable<IService> Services { get; set; }

        /// <summary>
        /// Creates new instance of service manager.
        /// </summary>
        static Manager()
        {
            
        }

        /// <summary>
        /// Initializes service manager.
        /// </summary>
        public static void Initialize()
        {
            LoadServices();
        }

        /// <summary>
        /// Loads services.
        /// </summary>
        /// <returns></returns>
        public static ICollection<T> GetService<T>()
        {
            var collection = new List<T>();

            try
            {
                foreach (var service in Services)
                {
                    if (service is T currentService)
                        collection.Add(currentService);
                }


            }
            catch (Exception e)
            {
                OnMessageReceived(new Message(e, false));
            }

            return collection;

        }

        /// <summary>
        /// Loads services.
        /// </summary>
        private static void LoadServices()
        {
            var assemblies = new List<Assembly>();

            var files = Directory.GetFiles(CurrentDirectory, "*.dll", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                try
                {
                    var hasItem = false;
                    var fileInfo = new FileInfo(file);

                    OnMessageReceived(new Message("Assembly loading", "Trying to load assembly " + fileInfo.Name, "Service manager", MessageType.Information));

                    foreach (var assembly in assemblies)
                    {
                        var name = assembly.GetName().Name;
                        if (name == fileInfo.Name.Replace(fileInfo.Extension, ""))
                            hasItem = true;
                    }

                    if (!hasItem)
                    {
                        assemblies.Add(AssemblyLoadContext.Default.LoadFromAssemblyPath(file));
                        OnMessageReceived(new Message("Assembly loading", "Assembly " + fileInfo.Name + " suitable for loading.", "Service manager", MessageType.Information));
                    }
                }
                catch (Exception e)
                {
                    OnMessageReceived(new Message(e, false));
                }
            }

            try
            {
                OnMessageReceived(new Message("Assembly loading", "Trying to load suitable assemblies.", "Service manager", MessageType.Information));

                var configuration = new ContainerConfiguration()
                    .WithAssemblies(assemblies);

                using var container = configuration.CreateContainer();
                Services = container.GetExports<IService>();

                OnMessageReceived(new Message("Assembly loading", "Suitable assemblies loaded.", "Service manager", MessageType.Information));
            }
            catch (Exception e)
            {
                OnMessageReceived(new Message(e, false));
            }
        }

        /// <summary>
        /// Notifies when message received.
        /// </summary>
        /// <param name="e">Message.</param>
        private static void OnMessageReceived(IMessage e)
        {
            MessageReceived?.Invoke(null, e);
        }
    }
}