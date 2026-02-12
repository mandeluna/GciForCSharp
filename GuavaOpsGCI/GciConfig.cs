namespace GuavaOpsGCI
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
        public string HostUserName { get; set; } = string.Empty;
        public string HostPassword { get; set; } = string.Empty;

        public string HostName { get; set; } = string.Empty;
        
        // The Gem service name/port (e.g., 'gemnetobject' or '!@localhost/netldi:50378')
        // THe Gem service and NetLDI NRS will be constructed by the GemStoneSession
        // e.g. !tcp@${Host}#netldi:${NetLDI}#task!gemnetobject
        public string StoneServer { get; set; } = string.Empty;
        public string NetLDI { get; set; } = "50378";

        // GemStone database credentials
        public string GsUserName { get; set; } = string.Empty;
        public string GsPassword { get; set; } = string.Empty;
        
        // Optional pooling configuration
        public PoolSettings Pool { get; set; } = new();
    }
}