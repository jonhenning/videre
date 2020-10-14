using CodeEndeavors.Extensions;
using CodeEndeavors.ServiceHost.Common.Services;
using CodeEndeavors.ServiceHost.Common.Services.Profiler;
using StackExchange.Profiling;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using Videre.Core.Services.Profiler;

namespace Videre.Core.Profilers
{
    public class StackExchangeTimeline : IProfileCapture, IServiceHostProfilerCapture
    {
        private IDisposable _step = null;
        private string _timingJson = null;
        private string _results = null;
        private decimal? _startMilliseconds = null;
        private Stopwatch _sw = new Stopwatch();
        public StackExchangeTimeline(string eventName)
        {
            _startMilliseconds = MiniProfiler.Current?.Head.StartMilliseconds;
            _sw.Start();
            _step = MiniProfiler.Current?.Step(eventName + " " + StackExchange.Profiling.Helpers.StackTraceSnippet.Get());
        }

        public string Results
        {
            get
            {
                if (_timingJson == null)
                    _timingJson = MiniProfiler.ToJson();
                return _timingJson;
            }
        }

        public void AppendResults(string results)
        {
            _results = results;
        }
        //public void AppendResults(string results)
        //{
        //    if (!string.IsNullOrEmpty(results))
        //    {
        //        var profiler = MiniProfiler.FromJson(results);
        //        //profiler?.Root?.Children.ForEach(child => MiniProfiler.Current?.Head?.AddChild(child));
        //        profiler?.GetTimingHierarchy().ToList().ForEach(child => MiniProfiler.Current?.Head?.AddChild(child));
        //    }
        //}

        public IDisposable CustomTiming(string category, string commandString)
        {
            IDisposable ret = (IDisposable)MiniProfiler.Current?.CustomTiming(category, commandString);
            return ret != null ? ret : new NoOpDisposable();
        }

        public void Dispose()
        {
            _step?.Dispose();
            _sw.Stop();
            if (!string.IsNullOrEmpty(_results))
            {
                //var profiler = MiniProfiler.FromJson(_results);
                var children = _results.ToObject<List<Timing>>();
                children.ForEach(child =>
                {
                    if (_startMilliseconds.HasValue)
                    {
                        child.StartMilliseconds = _startMilliseconds.Value;
                        child.DurationMilliseconds = _sw.ElapsedMilliseconds;
                    }
                    MiniProfiler.Current?.Head?.AddChild(child);
                });
            }
        }
    }
}