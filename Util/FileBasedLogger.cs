using Serilog;
using System;
using System.IO;                               // For Path, File, Directory, FileInfo
using System.Runtime.InteropServices;          // For RuntimeInformation and OSPlatform

namespace Util
{
    public enum LOG_ENUM_ERROR_TYPE
    {
        Unknown = -1,
        Info = 0,
        Warning,
        Error,
        Severe
    }

    public enum LOG_ENUM_ERROR_CATEGORY
    {
        Unknown = -1,
        User = 0,
        Data,
        Application,
        System
    }
    public static class FileBasedLogger
    {
        private static bool _isInitialized = false;

        // TODO: automatically pick a Windows path vs. a Linux path for log file
        public static void Initialize()
        {
            if (_isInitialized) return;

            string logPath;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Windows: Local app data or a relative folder
                logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "gemstone-log.csv");
            }
            else
            {
                // Linux/macOS: Standard logging directory
                logPath = "/var/log/cckinfinitytwo/gemstone-log.csv";
            }

            // Ensure the directory exists
            var directory = Path.GetDirectoryName(logPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Write header if the file is new/empty
            try
            {
                if (!File.Exists(logPath) || new FileInfo(logPath).Length == 0)
                {
                    File.WriteAllText(logPath, "Timestamp,Category,Type,Message" + Environment.NewLine);
                }
            }
            catch (Exception)
            {
                // Fail silently or fallback to console if file is locked
            }

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(logPath, 
                    rollingInterval: RollingInterval.Day,
                    hooks: null, // You can add hooks here for more complex CSV escaping if needed
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss},{Category},{Level},\"{Message:lj}\"{NewLine}{Exception}")
                .CreateLogger();

            _isInitialized = true;
        }

        /// <summary>
        /// Matches the legacy CCK LogEvent API but redirects to Serilog CSV
        /// </summary>
        public static void LogEvent(
            DateTime timestamp, 
            LOG_ENUM_ERROR_CATEGORY category, 
            LOG_ENUM_ERROR_TYPE type, 
            string message)
        {
            // Sanitize message: replace " with "" (Standard CSV escaping)
            string safeMessage = message?.Replace("\"", "\"\"") ?? string.Empty;

            // Enrich the log event with the Category before writing
            var log = Log.ForContext("Category", category.ToString());
            
            // Map our custom Type to Serilog's standard levels
            // Use the ?? "" trick to satisfy the .NET 9 null-checker
            switch (type)
            {
                case LOG_ENUM_ERROR_TYPE.Info: log.Information(message ?? ""); break;
                case LOG_ENUM_ERROR_TYPE.Warning: log.Warning(message ?? ""); break;
                case LOG_ENUM_ERROR_TYPE.Error: log.Error(message ?? ""); break;
                case LOG_ENUM_ERROR_TYPE.Severe: log.Fatal(message ?? ""); break;
                default: log.Debug(message ?? ""); break;
            }
        }

        // Convenience methods to make the code less verbose
        public static void DataWarning(string msg) => 
            LogEvent(DateTime.Now, LOG_ENUM_ERROR_CATEGORY.Data, LOG_ENUM_ERROR_TYPE.Warning, msg);

        public static void SystemError(string msg) => 
            LogEvent(DateTime.Now, LOG_ENUM_ERROR_CATEGORY.System, LOG_ENUM_ERROR_TYPE.Error, msg);

        public static void LogInformation(string msg) =>
            Log.Information(msg ?? "");
        
        public static void Shutdown() => Log.CloseAndFlush();
    }
}