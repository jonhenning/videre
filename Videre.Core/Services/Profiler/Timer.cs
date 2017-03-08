using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Videre.Core.Services.Profiler
{
    public class Timer : IDisposable
    {
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private readonly string _message;

        public Timer(string message)
        {
            _message = message;
            _stopwatch.Start();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            Services.Logging.Logger.DebugFormat("{0} - {1}ms", _message, _stopwatch.ElapsedMilliseconds);
            GC.SuppressFinalize(this);
        }
    }
}
