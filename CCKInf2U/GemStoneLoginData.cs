#nullable enable

using SparkSupport;

namespace CCKInf2U;

public sealed record GemStoneLoginData(
	string Host,
	string GemServer,
	string NetLDI,
	string Username,
	string Password,
	string HostUserName,
	string HostPassword,
	AtomicAccessLock? AccessLock);

#nullable restore