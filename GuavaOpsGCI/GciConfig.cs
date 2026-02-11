using System;

namespace GCI
{
    /// <summary>
    /// Settings for the GCI session pool managed by the GemStoneService.
    /// </summary>
    public class PoolSettings
    {
        public int MinSessions { get; set; } = 2;
        public int MaxSessions { get; set; } = 5;
    }

    /// <summary>
    /// Connectivity and authentication settings for the GemStone Stone and Gem.
    /// </summary>
    public class GciConfig
    {
        // Stone connection string (e.g., 'gs64stone')
        public string StoneName { get; set; } = string.Empty;
        
        // Host OS credentials (if required by the Gem service)
        public string HostUserId { get; set; } = string.Empty;
        public string HostPassword { get; set; } = string.Empty;

        public string HostName { get; set; } = string.Empty;
        
        // The Gem service name/port (e.g., 'gemnetobject' or '!@localhost/netldi:50378')
        public string GemService { get; set; } = string.Empty;
        
        // GemStone database credentials
        public string GsUserName { get; set; } = string.Empty;
        public string GsPassword { get; set; } = string.Empty;
        
        // Optional pooling configuration
        public PoolSettings Pool { get; set; } = new();
    }
}