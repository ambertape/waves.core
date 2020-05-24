﻿using System;

namespace Waves.Core.Base.Interfaces
{
    /// <summary>
    ///     Interfaces of Waves's message structures.
    /// </summary>
    public interface IMessage : IMessageObject
    {
        /// <summary>
        ///     Gets text of the message.
        /// </summary>
        string Text { get; }

        /// <summary>
        ///     Gets exception.
        /// </summary>
        Exception Exception { get; }
    }
}