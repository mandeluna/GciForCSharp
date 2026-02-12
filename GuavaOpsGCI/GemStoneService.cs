using CCKInf2U;
using CCKInf2U.ThreadSafe;
using Microsoft.Extensions.Options;

namespace GuavaOpsGCI
{
    /// <summary>
    /// Service managed by the Web Container to handle GemStone requests.
    /// Maps Read-Only and Read-Write transactional patterns to HTTP semantics.
    /// </summary>
    public class GemStoneService : IDisposable
    {
        private readonly GciConfig _config;
        private readonly SemaphoreSlim _poolLock;
        private GemStoneSession _session;
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

                // string Host          (gem server actually)
                // string GemServer     (stone server)
                // string NetLDI
                // string Username
                // string Password
                // string HostUserName
                // string HostPassword
                GemStoneLoginData parameters = new GemStoneLoginData(
                    _config.HostName,
                    _config.StoneServer,    // not necessarily the GemServer
                    _config.NetLDI,
                    _config.GsUserName,
                    _config.HostPassword,
                    _config.GsUserName,
                    _config.HostPassword,
                    null);
                _session = GemStoneSession.CreateGemStoneConnection(parameters.AccessLock);
                _session.LoginAsUser(parameters);

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
                    _session.BeginTransaction();
                    var gemStoneObject = _session.ExecuteString(wrappedSource);
                    
                    if (requireCommit)
                    {
                        if (!_session.CommitTransaction())
                        {
                            _session.AbortTransaction();
                            throw new Exception("GemStone Commit Conflict: Data was modified by another session.");
                        }
                    }
                    else
                    {
                        // For read-only, we abort to release any read-set tracking in the gem/stone
                        _session.AbortTransaction();
                    }
                    bool isWrongType = false;
                    string stringResult = gemStoneObject.AsString(ref isWrongType);
                    if (isWrongType)
                    {
                        throw new Exception("Expression result was not a string.");
                    }

                    return stringResult;
                });
            }
            finally
            {
                _poolLock.Release();
            }
        }

        public void Dispose()
        {
            if (_initialized) _session.Logout();
            _poolLock.Dispose();
        }
    }
}
