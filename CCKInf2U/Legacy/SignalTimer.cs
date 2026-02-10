using System.Runtime.CompilerServices;

namespace CCKInf2U;

internal sealed class SignalTimer
{
	private readonly GemStoneSession _session;
	private CCKXTIMERS.XTimer _timer;

	private CCKXTIMERS.XTimer Timer
	{
		[MethodImpl(MethodImplOptions.Synchronized)]
		get
		{
			return _timer;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		set
		{
			if (_timer != null)
			{
				_timer.Tick -= CheckGemStoneSignal;
			}

			_timer = value;
			if (_timer != null)
			{
				_timer.Tick += CheckGemStoneSignal;
			}
		}
	}

	public SignalTimer(GemStoneSession session)
	{
		_session = session;
		Timer = new()
		{
			Interval = 1000
		};
	}

	public void EnableSignals(bool pbEnable)
	{
		Timer.Enabled = pbEnable;
	}

	private void CheckGemStoneSignal()
	{
		if (_session.PollForSignal())
		{
			var lbError = default(bool);
			_session.CheckError(ref lbError);
		}
		_session.CheckSignal();
	}
}