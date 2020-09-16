using CodeEndeavors.Extensions;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Framework;
using Glimpse.Core.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Videre.Core.Services.Profiler
{
    //http://stackoverflow.com/a/28152114
    public static class Timeline
    {
        private static string _profilerName = Portal.GetAppSetting("Profiler.Name", "") + "Timeline";
        private static Type _captureType = null;

        public static IDisposable Capture(string eventName)
        {
            if (_captureType == null)
            {
                var types = typeof(IProfileCapture).GetAllTypes();
                _captureType = types.Where(t => t.Name == _profilerName).FirstOrDefault();
            }
            if (_captureType != null)
                return (IProfileCapture)Activator.CreateInstance(_captureType, eventName);
            return new NoOpProfileCapture(eventName);
        }
    }

    public class NoOpProfileCapture : IProfileCapture
    {

        public NoOpProfileCapture(string eventName)
        {
            //
        }
        public void Dispose()
        {

        }
    }

    public interface IProfileCapture : IDisposable 
    {
    };


}