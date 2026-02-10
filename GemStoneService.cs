using System.Runtime.InteropServices;
using Microsoft.Extensions.Options;
using CCKInf2U;

namespace GCI
{
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

                GemStoneLoginData parameters = new GemStoneLoginData(_config.HostName, _config.GemServer, _config.NetLDI,
                _config.Username, _config.Password, _config.HostUserName, _config.HostPassword);
                GemStoneSession session = parameters.login();

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
                    if (!GemStoneSession.currentSession.beginTransaction())
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
}
