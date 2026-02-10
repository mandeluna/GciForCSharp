using CCKInf2U.Interop;
using System;
using System.Threading;
using System.Threading.Tasks;
using static CCKInf2U.GemStoneSession;

namespace CCKInf2U.ThreadSafe;
public class GemStoneCullableInteraction
{
	private readonly GemStoneSession _session;
	private readonly CancellationToken _ct;

	public GemStoneCullableInteraction(GemStoneSession session, CancellationToken ct)
	{
		_session = session;
		_ct = ct;
	}


	public Task<GemStoneObject> ExecuteAsync(ReadOnlySpan<byte> command)
	{
		if (!_session.NbExecute(command))
		{
			return Task.FromResult<GemStoneObject>(new GemStoneObject(_session, GciOop.OOP_NIL));
		}

		return PollForEndOfInteractionAsync();
	}

	public Task<GemStoneObject> ForeignPergormWithArgsAsync(Oop receiverOop,
		ReadOnlySpan<byte> selector, ReadOnlySpan<Oop> args)
	{
		if (!_session.NbForeignPerform(receiverOop, selector, args))
		{
			return Task.FromResult<GemStoneObject>(new GemStoneObject(_session, GciOop.OOP_NIL));
		}

		return PollForEndOfInteractionAsync();
	}

	public Task<GemStoneObject> ForeignPerformAsync(Oop receiverOop, ReadOnlySpan<byte> selector)
	{
		if (!_session.NbForeignPerform(receiverOop, selector, ReadOnlySpan<Oop>.Empty))
		{
			return Task.FromResult<GemStoneObject>(new GemStoneObject(_session, GciOop.OOP_NIL));
		}

		return PollForEndOfInteractionAsync();
	}

	public Task<GemStoneObject> PollForEndOfInteractionAsync()
	{

		while ((_session.IsResultReady(100) == StatusOfNbCall.ResultNotReady) && (!_ct.IsCancellationRequested))
		{
			// Spin spin spin.
		}
		if (_ct.IsCancellationRequested)
		{
			_session.SoftBreak();
			_session.NbResultAcceptingBreaks();

			return Task.FromResult(new GemStoneObject(_session, GciOop.OOP_NIL));
		}

		var result = _session.NbResultAcceptingBreaks();

		return result.Oop == GciOop.OOP_ILLEGAL
			? Task.FromResult(new GemStoneObject(_session, GciOop.OOP_NIL))
			: Task.FromResult(result);
	}

}
