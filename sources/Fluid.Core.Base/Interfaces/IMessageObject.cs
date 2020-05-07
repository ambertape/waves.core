﻿using System;
using Fluid.Core.Base.Enums;

namespace Fluid.Core.Base.Interfaces
{
    /// <summary>
    /// Interface for message object.
    /// </summary>
    public interface IMessageObject
    {
        /// <summary>
        /// Gets ID of message object.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        ///     Gets title of the message object.
        /// </summary>
        string Title { get; }

        /// <summary>
        ///     Gets datetime of the message object.
        /// </summary>
        DateTime DateTime { get; }

        /// <summary>
        ///     Gets type of this message.
        /// </summary>
        MessageType Type { get; }
    }
}