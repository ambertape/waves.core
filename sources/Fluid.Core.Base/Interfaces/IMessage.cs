﻿using System;
using Fluid.Core.Base.Enums;

namespace Fluid.Core.Base.Interfaces
{
    /// <summary>
    ///     Interfaces of fluid's message structures.
    /// </summary>
    public interface IMessage : IMessageObject
    {
        /// <summary>
        ///     Gets text of the message.
        /// </summary>
        string Text { get; }

        /// <summary>
        ///     Gets sender of this message.
        /// </summary>
        string Sender { get; }

        /// <summary>
        ///     Gets exception.
        /// </summary>
        Exception Exception { get; }

        /// <summary>
        ///     Gets type of this message.
        /// </summary>
        MessageType Type { get; }
    }
}