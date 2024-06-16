using System.Diagnostics;

namespace ProcDumpEx
{
	internal class ThreadingTimer : IDisposable
	{
		private readonly Timer _timer;
		private readonly bool _autoReset;
		public TimeSpan TimeSpan { get; }
		private readonly Action _action;

		private DateTime? _endTime;
		public DateTime? EndTime => _endTime?.ToLocalTime();

		public ThreadingTimer(Action callback, bool autoReset, TimeSpan span)
		{
			_timer = new Timer(TimerCallback);
			_autoReset = autoReset;
			_action = callback;
			TimeSpan = span;
		}

		public void Start()
		{
			_endTime = DateTime.UtcNow + TimeSpan;
			var span = GetSpan();
			Debug.Assert(span is not null);
			_timer.Change(span.Value, Timeout.InfiniteTimeSpan);
		}

		public void Stop()
		{
			_endTime = null;
			_timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
		}

		public TimeSpan? GetSpan()
		{
			Debug.Assert(_endTime is not null);

			if (DateTime.UtcNow >= _endTime)
			{
				return null;
			}

			var span = _endTime - DateTime.UtcNow;

			if (span.Value < TimeSpan.Zero)
			{
				return null;
			}

			if (span.Value.TotalMilliseconds >= (uint.MaxValue - 1))
			{
				return TimeSpan.FromMilliseconds((uint.MaxValue - 1));
			}
			return span;
		}

		public void TimerCallback(object? obj)
		{
			var span = GetSpan();

			if (span is null)
			{
				_action();
				if (_autoReset)
				{
					Start();
				}
			}
			else
			{
				_timer.Change(span.Value, Timeout.InfiniteTimeSpan);
			}
		}

		public void Dispose()
		{
			Stop();
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			_timer.Dispose();
		}
	}
}
