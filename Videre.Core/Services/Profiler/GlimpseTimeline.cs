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
        public static IDisposable Capture(string eventName)
        {
#pragma warning disable 618
            var timer = GlimpseConfiguration.GetConfiguredTimerStrategy()();
            if (timer == null)
                return null;
            var broker = GlimpseConfiguration.GetConfiguredMessageBroker();
            if (broker == null)
                return null;
#pragma warning restore 618
            return new TimelineCapture(timer, broker, eventName);
        }

    }

    public class TimelineCapture : IDisposable
    {
        private readonly string _eventName;
        private readonly IExecutionTimer _timer;
        private readonly IMessageBroker _broker;
        private readonly TimeSpan _startOffset;

        public TimelineCapture(IExecutionTimer timer, IMessageBroker broker, string eventName)
        {
            _timer = timer;
            _broker = broker;
            _eventName = eventName;
            _startOffset = _timer.Start();
        }

        public void Dispose()
        {
            _broker.Publish(new TimelineMessage(_eventName, _timer.Stop(_startOffset)));
        }
    }

    public class TimelineMessage : ITimelineMessage
    {
        private static readonly TimelineCategoryItem DefaultCategory = new TimelineCategoryItem("MyApp", "green", "blue");

        public TimelineMessage(string eventName, TimerResult result)
        {
            Id = Guid.NewGuid();
            EventName = eventName;
            EventCategory = DefaultCategory;
            Offset = result.Offset;
            StartTime = result.StartTime;
            Duration = result.Duration;
        }

        public Guid Id { get; private set; }
        public TimeSpan Offset { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime StartTime { get; set; }
        public string EventName { get; set; }
        public TimelineCategoryItem EventCategory { get; set; }
        public string EventSubText { get; set; }
    }
}