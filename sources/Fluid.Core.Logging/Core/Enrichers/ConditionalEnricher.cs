﻿// Copyright 2019 Serilog Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using Fluid.Core.Logging.Events;

namespace Fluid.Core.Logging.Core.Enrichers
{
    internal class ConditionalEnricher : ILogEventEnricher, IDisposable
    {
        private readonly Func<LogEvent, bool> _condition;
        private readonly ILogEventEnricher _wrapped;

        public ConditionalEnricher(ILogEventEnricher wrapped, Func<LogEvent, bool> condition)
        {
            _wrapped = wrapped ?? throw new ArgumentNullException(nameof(wrapped));
            _condition = condition ?? throw new ArgumentNullException(nameof(condition));
        }

        public void Dispose()
        {
            (_wrapped as IDisposable)?.Dispose();
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (_condition(logEvent))
                _wrapped.Enrich(logEvent, propertyFactory);
        }
    }
}