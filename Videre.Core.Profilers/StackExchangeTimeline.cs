using CodeEndeavors.Extensions;
using StackExchange.Profiling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Videre.Core.Services.Profiler;

namespace Videre.Core.Profilers
{
    public class StackExchangeTimeline : IProfileCapture, CodeEndeavors.ServiceHost.Common.Services.Profiler.IServiceHostProfilerCapture
    {
        CustomTiming _timing = null;
        private string _timingJson = null;
        private Timing _serverTiming = null;
        public StackExchangeTimeline(string eventName)
        {
            if (MiniProfiler.Current != null)
                _timing = MiniProfiler.Current.CustomTiming("Videre", eventName);
        }

        public string Results
        {
            get
            {
                if (_timing != null)
                {
                    _timing.Stop();
                    if (MiniProfiler.Current != null && MiniProfiler.Current.Root != null)
                        _timingJson = MiniProfiler.Current.Root.ToJson();
                    _timing = null;
                }
                return _timingJson;
            }
        }

        public void AppendResults(string results)
        {
            //try
            //{
            if (!string.IsNullOrEmpty(results))
            {
                _serverTiming = results.ToObject<Timing>();
            }
            //}
            //catch (Exception ex)
            //{
            //TODO: LOG IT
            //}
        }

        public void Step(string name)
        {
            MiniProfiler.Current?.Step(name);
        }

        public void Dispose()
        {
            if (_timing != null)
            {
                _timing.Stop();
                _timing = null;
            }
            if (_serverTiming != null && MiniProfiler.Current != null && MiniProfiler.Current.Root != null)
            {
                MiniProfiler.Current.Root.AddChild(_serverTiming);
            }
        }
    }
}