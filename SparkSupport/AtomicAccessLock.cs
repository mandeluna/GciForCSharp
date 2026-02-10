using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SparkSupport;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class AssumeSessionLockedAttribute : Attribute
{
	// To be supported by an analyser later -> ensuring the caller is either marked with AssumeSessionLocked itself
	// or is actually holding the lock when the call is made. If violating this is a compile-time exception
	// we wouldn't have to pass the lock around for clarity. For now, we'll rely on code review.
}

public sealed class AtomicAccessLock : IDisposable
{
	private readonly SemaphoreSlim _lock = new(1, 1);
	public string CurrentHolder { get; private set; } = string.Empty;
	private readonly ActivitySource _activitySource = new ActivitySource(nameof(AtomicAccessLock));

	public bool PeekIsLock()
	{
		return _lock.CurrentCount == 0;
	}

	public RefAccessLock AcquireLock([CallerMemberName] string callerMemberName = "")
	{
		while (!_lock.Wait(TimeSpan.FromSeconds(10)))
		{
			Thread.Sleep(1_000);
		}

		CurrentHolder = callerMemberName;
		return new(_lock);
	}

	public async Task<AccessLock> AcquireLockAsync(CancellationToken ct = default, [CallerMemberName] string callerMemberName = "",
		Action? releaseLockCallback = null)
	{
		using (var waitingForLock = _activitySource.StartActivity($"Waiting for lock - {callerMemberName}"))
		{
			while (!await _lock.WaitAsync(TimeSpan.FromSeconds(10), ct))
			{
				await Task.Delay(1_000, ct);
			}
		}

		CurrentHolder = callerMemberName;
		return new AccessLock(_lock, releaseLockCallback);
	}

	public void Dispose()
	{
		_lock.Dispose();
	}

	/// <summary>
	/// Temporary method to unlock a deadlocked bucket.
	/// </summary>
	/// <remarks>
	/// This is <b>not</b> production code, don't use it in anything permanent.
	/// </remarks>
	/// <returns>
	/// Returns basic diagnostic text.
	/// </returns>
	public string UnsafeReleaseLock()
	{
		if (_lock.CurrentCount == 0)
		{
			try
			{
				_ = _lock.Release();
				return $"Lock released, {CurrentHolder} has had their lock removed.";
			}
			catch (SemaphoreFullException)
			{
				// We tried to be sure someone had it, but we were wrong a split second later.
				return $"Lock was released *just* before this call, the last holder was {CurrentHolder}";
			}
		}

		return $"Lock isn't currently held, the last holder was {CurrentHolder}";
	}
}

public readonly ref struct RefAccessLock(SemaphoreSlim sessionLock)
{
	public void Dispose()
	{
		try
		{
			_ = sessionLock.Release();
		}
		catch (SemaphoreFullException)
		{
			// Swallow. This try-catch will be removed when AtomicAccessLock.UnsafeReleaseLock is removed.
		}
	}
}

public readonly struct AccessLock : IDisposable
{
	private readonly SemaphoreSlim _sessionLock;
	private static readonly ActivitySource _activitySource = new(nameof(AccessLock));
	private readonly Activity? _activity;
	private readonly Action? _releaseLockCallback;
	public AccessLock(SemaphoreSlim sessionLock, Action? releaseLockCallback = null)
	{
		_sessionLock = sessionLock;
		_releaseLockCallback = releaseLockCallback;
		_activity = _activitySource.StartActivity("Using Lock");
	}

	public void Dispose()
	{
		try
		{
			_releaseLockCallback?.Invoke();
		}
		catch (Exception ex)
		{
			Debugger.Break();
			//TODO Log this
		}
		_activity?.Dispose();
		try
		{
			_ = _sessionLock.Release();
		}
		catch (SemaphoreFullException)
		{
			// Swallow. This try-catch will be removed when AtomicAccessLock.UnsafeReleaseLock is removed.
		}
	}
}
