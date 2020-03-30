﻿using System;
using System.ComponentModel;

namespace Fluid.Core.Base.Interfaces
{
    /// <summary>
    ///     Interface for connection classes.
    /// </summary>
    public interface IConnection : IObject, INotifyPropertyChanged, IDisposable, ICloneable
    {
        /// <summary>
        ///     Get input entry point of this connection.
        /// </summary>
        IEntryPoint Input { get; set; }

        /// <summary>
        ///     Gets output entry point of this connection.
        /// </summary>
        IEntryPoint Output { get; set; }
    }
}