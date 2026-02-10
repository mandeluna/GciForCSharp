using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Util.Shims;

public class DrMessageBoxListener
{
	private ConcurrentQueue<MessageBoxAttempt> _que = new ();
	public IForwarder? Forwarder;
	public void Enque(MessageBoxAttempt messageBoxAttempt)
	{
		Forwarder?.Forward(messageBoxAttempt);
		_que.Enqueue(messageBoxAttempt);
	}

	public void Clear()
	{
		_que.Clear();
	}
}
