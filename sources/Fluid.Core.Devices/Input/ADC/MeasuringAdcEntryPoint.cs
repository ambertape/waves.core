﻿using Fluid.Core.Base.Interfaces;
using Fluid.Core.Devices.Interfaces.Input.ADC;

namespace Fluid.Core.Devices.Input.ADC
{
    public abstract class MeasuringAdcEntryPoint : AdcEntryPoint, IMeasuringAdcEntryPoint
    {
        private double _digitalResolution;
        private double _minimumVoltage;
        private double _maximumVoltage;
        private bool _isPhantomPowerSupported;

        /// <inheritdoc />
        protected MeasuringAdcEntryPoint(IModule parent) : base(parent)
        {
        }

        /// <inheritdoc />
        public double DigitalResolution
        {
            get => _digitalResolution;
            private set
            {
                if (value.Equals(_digitalResolution)) return;
                _digitalResolution = value;
                OnPropertyChanged();
            }
        }

        /// <inheritdoc />
        public double MinimumVoltage
        {
            get => _minimumVoltage;
            private set
            {
                if (value.Equals(_minimumVoltage)) return;
                _minimumVoltage = value;
                OnPropertyChanged();
            }
        }

        /// <inheritdoc />
        public double MaximumVoltage
        {
            get => _maximumVoltage;
            private set
            {
                if (value.Equals(_maximumVoltage)) return;
                _maximumVoltage = value;
                OnPropertyChanged();
            }
        }

        /// <inheritdoc />
        public bool IsPhantomPowerSupported
        {
            get => _isPhantomPowerSupported;
            private set
            {
                if (value == _isPhantomPowerSupported) return;
                _isPhantomPowerSupported = value;
                OnPropertyChanged();
            }
        }

        /// <inheritdoc />
        public abstract bool IsPhantomPowerEnabled { get; set; }
    }
}