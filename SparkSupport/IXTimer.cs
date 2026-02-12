using System;

namespace SparkSupport
{
    /// <summary>
    /// Defines a cross-platform timer interface that replaces the legacy WinForms dependencies.
    /// </summary>
    public interface IXTimer : IDisposable
    {
        double Interval { get; set; }
        bool Enabled { get; set; }
        
        /// <summary>
        /// Occurs when the interval elapses.
        /// </summary>
        event Action Tick;
    }
}