using Glimpse.Core.Extensibility;
using Glimpse.Core.Framework;
using Glimpse.Core.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Videre.Core.Services.Profiler
{
    
    
    public class GlimpseTimeline : IProfileCapture
    {
        private readonly string _eventName;
        private readonly IExecutionTimer _timer;
        private readonly IMessageBroker _broker;
        private readonly TimeSpan _startOffset;

        public GlimpseTimeline(string eventName)
        {
#pragma warning disable 618
            _timer = GlimpseConfiguration.GetConfiguredTimerStrategy()();
            _broker = GlimpseConfiguration.GetConfiguredMessageBroker();
#pragma warning restore 618
            _eventName = eventName;
        }

        public GlimpseTimeline(IExecutionTimer timer, IMessageBroker broker, string eventName)
        {
            _timer = timer;
            _broker = broker;
            _eventName = eventName;
            if (_timer != null)
                _startOffset = _timer.Start();
        }

        public void Dispose()
        {
            if (_broker != null && _timer != null)
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