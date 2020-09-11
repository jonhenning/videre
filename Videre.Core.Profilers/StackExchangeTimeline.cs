using StackExchange.Profiling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Videre.Core.Services.Profiler;

namespace Videre.Core.Profilers
{
    public class StackExchangeTimeline : IProfileCapture
    {
        CustomTiming _timing = null;
        public StackExchangeTimeline(string eventName)
        {
            _timing = MiniProfiler.Current.CustomTiming("Videre", eventName);
        }

        public void Dispose()
        {
            if (_timing != null)
            {
                _timing.Stop();
                _timing = null;
            }
        }
    }
}