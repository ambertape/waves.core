﻿using System.Collections.Generic;
using Ambertape.Core.Base.Interfaces;

namespace Ambertape.Core.Devices.Interfaces.Input.ADC
{
    /// <summary>
    ///     Interface for ADC entry point.
    /// </summary>
    public interface IAdcEntryPoint : IEntryPoint
    {
        /// <summary>
        ///     Gets digital gain.
        /// </summary>
        double DigitalGain { get; set; }

        /// <summary>
        ///     Gets device gain.
        /// </summary>
        double DeviceGain { get; set; }

        /// <summary>
        ///     Gets available device gains collection.
        /// </summary>
        ICollection<double> AvailableDeviceGains { get; }
    }
}