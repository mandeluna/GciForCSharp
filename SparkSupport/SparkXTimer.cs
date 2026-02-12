using Timer = System.Timers.Timer;

namespace SparkSupport
{
    /// <summary>
    /// A thread-safe, cross-platform timer for GemStone session management.
    /// Replaces System.Windows.Forms.Timer with System.Timers.Timer.
    /// </summary>
    public class SparkXTimer : IXTimer
    {
        private readonly Timer _timer;
        private readonly object _accessLock;
        private readonly string _name;

        /// <summary>
        /// Implementation of the IXTimer Tick event.
        /// </summary>
        public event Action? Tick;

        /// <param name="accessLock">The synchronization lock (AtomicAccessLock) from the GemStoneSession.</param>
        /// <param name="name">A descriptive name for debugging/logging.</param>
        public SparkXTimer(object accessLock, string name)
        {
            _accessLock = accessLock ?? throw new ArgumentNullException(nameof(accessLock));
            _name = name;
            
            _timer = new Timer();
            _timer.AutoReset = true;
            _timer.Elapsed += (sender, args) => HandleElapsed();
        }

        public double Interval 
        { 
            get => _timer.Interval; 
            set => _timer.Interval = value; 
        }

        public bool Enabled 
        { 
            get => _timer.Enabled; 
            set => _timer.Enabled = value; 
        }

        /// <summary>
        /// Internal handler for the system timer. 
        /// Ensures the Tick event is synchronized with the GemStone session's activity.
        /// </summary>
        private void HandleElapsed()
        {
            // We lock on the session's accessLock. 
            // This prevents the "Forgive" logic (resetting counters) or any other timer side-effects
            // from executing while a worker thread is mid-GCI-call.
            lock (_accessLock)
            {
                try
                {
                    Tick?.Invoke();
                }
                catch (Exception ex)
                {
                    // Log the error but do not allow it to crash the ThreadPool thread.
                    Console.WriteLine($"[SparkXTimer:{_name}] Error during synchronized Tick: {ex.Message}");
                }
            }
        }

        public void Dispose()
        {
            _timer.Stop();
            _timer.Dispose();
            // Clear handlers to avoid memory leaks or "ghost" events
            Tick = null; 
        }
    }
}