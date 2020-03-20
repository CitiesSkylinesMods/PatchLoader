namespace Patch.API
{
    /// <summary>
    /// Simple Logger wrapper interface
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Info logger 
        /// </summary>
        /// <param name="message">Info message to log</param>
        void Info(string message);

        /// <summary>
        /// Error logger
        /// </summary>
        /// <param name="message">Error message to log</param>
        void Error(string message);

        /// <summary>
        /// Log entry prefix. Concatenated to <see cref="Info"/> and <see cref="Error"/>
        /// Should be set up in <see cref="IPatch.Execute"/>
        /// </summary>
        string Prefix { get; set; }
    }
}
