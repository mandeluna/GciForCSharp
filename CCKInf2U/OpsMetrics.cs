using System.Collections.Generic;
using System.Diagnostics.Metrics;

namespace CCKInf2U;
public class OpsMetrics
{
	private readonly Counter<int>? _opsIneteractionCounter;

	public static readonly string MeterName = "Ops.Interactions";

	public OpsMetrics()
	{
		_opsIneteractionCounter = null;
	}
	public OpsMetrics(IMeterFactory meterFactory)
	{
		var meter = meterFactory.Create(MeterName);
		_opsIneteractionCounter = meter.CreateCounter<int>("ops","Counts the number of ops calls");
	}

	public void GemstoneForeignFunctionCall(string selector)
	{
		_opsIneteractionCounter?.Add(1,
			new KeyValuePair<string, object?>("ops.ffi.selector", selector));
	}

	public void GemstoneExecution(string script)
	{
		// currently metrics are not monitored.
		_opsIneteractionCounter?.Add(1,
			new KeyValuePair<string, object?>($"ops.execution.script", script));
	}
}
