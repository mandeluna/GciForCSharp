using System.Runtime.InteropServices;
using Microsoft.Extensions.Options;

namespace GCI
{
    public class PoolSettings
    {
        public int MinSessions { get; set; } = 2;
        public int MaxSessions { get; set; } = 5;
    }

    public class GciConfig
    {
        public required string StoneName { get; set; }
        public string? HostUserId { get; set; }
        public string? HostPassword { get; set; }
        public required string GemService { get; set; }
        public required string GsUserName { get; set; }
        public required string GsPassword { get; set; }
        public PoolSettings? Pool { get; set; }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct GciErrSType
    {
        public IntPtr category; 
        public int number;
        public int argCount;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1025)]
        public string message;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public long[] args;
    }

    /// <summary>
    /// Service managed by the Web Container to handle GemStone requests.
    /// Maps Read-Only and Read-Write transactional patterns to HTTP semantics.
    /// </summary>
    public class GemStoneService : IDisposable
    {
        private readonly GciConfig _config;
        private readonly SemaphoreSlim _poolLock;
        private bool _initialized = false;

        public GemStoneService(IOptions<GciConfig> config)
        {
            _config = config.Value;
            // The semaphore controls the max number of concurrent GCI operations allowed.
            _poolLock = new SemaphoreSlim(_config.Pool?.MaxSessions ?? 1);
        }

        private void EnsureInitialized()
        {
            if (_initialized) return;

            lock (this)
            {
                if (_initialized) return;

                if (!GciWrapper.GciInit())
                    throw new Exception("GciInit failed");

                GciWrapper.GciSetNet(_config.StoneName, _config.HostUserId, _config.HostPassword, _config.GemService);

                if (!GciWrapper.GciLogin(_config.GsUserName, _config.GsPassword))
                    throw new Exception("GciLogin failed during service startup.");

                _initialized = true;
            }
        }

        /// <summary>
        /// For HTTP POST/PUT/DELETE: Executes code intended to change database state.
        /// Pattern: Begin -> Execute -> Commit.
        /// </summary>
        public async Task<string> CallReadWriteAsync(string smalltalkSource)
        {
            return await ExecuteInternalAsync(smalltalkSource, requireCommit: true);
        }

        /// <summary>
        /// For HTTP GET: Executes code intended for side-effect-free queries.
        /// Pattern: Begin (refresh view) -> Execute -> Abort (clean state).
        /// Avoids the concurrency overhead and stone round-trip of a commit.
        /// </summary>
        public async Task<string> CallReadOnlyAsync(string smalltalkSource)
        {
            return await ExecuteInternalAsync(smalltalkSource, requireCommit: false);
        }

        private async Task<string> ExecuteInternalAsync(string smalltalkSource, bool requireCommit)
        {
            EnsureInitialized();

            // Wrap in STON to ensure a clean JSON string is returned to C#
            string wrappedSource = $@"
                | result |
                result := [ {smalltalkSource} ] value.
                ^ STON toJsonString: result";

            await _poolLock.WaitAsync();
            try
            {
                return await Task.Run(() =>
                {
                    // Ensure the session has an up-to-date view of the repository
                    if (!GciWrapper.GciBeginTransaction())
                        throw new Exception("Failed to begin transaction.");

                    long resultOop = GciWrapper.Execute(wrappedSource);
                    
                    if (resultOop == GciWrapper.OOP_NIL)
                    {
                        GciWrapper.GciAbort();
                        if (GciWrapper.GciErr(out GciErrSType err))
                            throw new Exception($"GemStone Error {err.number}: {err.message}");
                        throw new Exception("Execution returned NIL (unspecified error).");
                    }

                    if (requireCommit)
                    {
                        if (!GciWrapper.GciCommit())
                        {
                            GciWrapper.GciAbort();
                            throw new Exception("GemStone Commit Conflict: Data was modified by another session.");
                        }
                    }
                    else
                    {
                        // For read-only, we abort to release any read-set tracking in the gem/stone
                        GciWrapper.GciAbort();
                    }

                    return GciWrapper.GetGsString(resultOop);
                });
            }
            finally
            {
                _poolLock.Release();
            }
        }

        public void Dispose()
        {
            if (_initialized) GciWrapper.GciLogout();
            _poolLock.Dispose();
        }
    }

    public static class GciWrapper
    {
        private const string LibName = "libgcirpc";
        public const long OOP_NIL = 20;

        static GciWrapper()
        {
            NativeLibrary.SetDllImportResolver(typeof(GciWrapper).Assembly, (libraryName, assembly, searchPath) =>
            {
                string gemstonePath = Environment.GetEnvironmentVariable("GEMSTONE");
                if (string.IsNullOrEmpty(gemstonePath)) return IntPtr.Zero;

                string libDir = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "bin" : "lib";
                string binPath = Path.Combine(gemstonePath, libDir);
                string ext = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".dll" : ".so";
                
                // Search for the library with wildcard to handle versioning like libgcirpc-3.7.1.dll
                string[] files = Directory.GetFiles(binPath, $"{libraryName}*{ext}");
                return files.Length > 0 ? NativeLibrary.Load(files[0]) : IntPtr.Zero;
            });
        }

        #region P/Invoke Definitions
        [DllImport(LibName)] public static extern bool GciInit();
        [DllImport(LibName)] public static extern void GciSetNet(string s, string u, string p, string g);
        [DllImport(LibName)] public static extern bool GciLogin(string u, string p);
        [DllImport(LibName)] public static extern void GciLogout();
        [DllImport(LibName)] public static extern bool GciErr(out GciErrSType e);
        [DllImport(LibName)] public static extern long GciNewString(string s);
        [DllImport(LibName)] public static extern long GciExecute(long s, long sym);
        [DllImport(LibName)] public static extern bool GciBeginTransaction();
        [DllImport(LibName)] public static extern bool GciCommit();
        [DllImport(LibName)] public static extern void GciAbort();
        [DllImport(LibName)] public static extern long GciGetSize(long oop);

        [DllImport(LibName, CharSet = CharSet.Ansi)] 
        private static extern int GciFetchChars(long oop, long startIndex, IntPtr buffer, long bufferSize);
        #endregion

        public static long Execute(string source)
        {
            return GciExecute(GciNewString(source), OOP_NIL);
        }

        public static string GetGsString(long oop)
        {
            long size = GciGetSize(oop);
            if (size <= 0) return string.Empty;

            IntPtr buf = Marshal.AllocHGlobal((int)size);
            try
            {
                int fetched = GciFetchChars(oop, 1, buf, size);
                return Marshal.PtrToStringAnsi(buf, fetched);
            }
            finally
            {
                Marshal.FreeHGlobal(buf);
            }
        }
    }
}