using System.Windows.Threading;

namespace PKEngineEditor.Utilities
{
    public static class ID
    {
        public static int INVALID_ID => -1;

        public static bool IsValid(int id) => id != INVALID_ID;
    }

    public static class MathUtil
    {
        public static float Epsilon => 0.00001f;

        public static bool IsTheSameAs(this float value, float other)
        {
            return Math.Abs(value - other) < Epsilon;
        }

        public static bool IsTheSameAs(this float? value, float? other)
        {
            if (!value.HasValue || !other.HasValue) return false;
            return Math.Abs(value.Value - other.Value) < Epsilon;
        }
    }

    public class DelayEventTimerArgs : EventArgs
    {
        public bool   RepeatEvent { get; set; }
        public Object Data        { get; set; }

        public DelayEventTimerArgs(Object data)
        {
            Data = data;
        }
    }

    public class DelayEventTimer
    {
        private readonly DispatcherTimer               _timer;
        private readonly TimeSpan                      _delay;
        private          DateTime                      _lastEventTime = DateTime.Now;
        private          object                        _data          = null!;
        public event EventHandler<DelayEventTimerArgs> Triggers       = null!;

        public void Trigger(object data = null!)
        {
            _data            = data;
            _lastEventTime   = DateTime.Now;
            _timer.IsEnabled = true;
        }

        public void Disable()
        {
            _timer.IsEnabled = false;
        }

        public DelayEventTimer(TimeSpan delay, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            _delay = delay;
            _timer = new DispatcherTimer(priority)
            {
                Interval = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 0.25)
            };
            _timer.Tick += OnTimerTick;
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            if (DateTime.Now - _lastEventTime < _delay) return;
            var eventArgs = new DelayEventTimerArgs(_data);
            Triggers.Invoke(this, eventArgs);
            _timer.IsEnabled = eventArgs.RepeatEvent;
        }
    }
}