namespace ContainerExpressions.Containers
{
    /// <summary>Allows the use of Trace, and Try logs; without needed to start with some value of {T}, or Response{T}.</summary>
    public static class Log
    {
        /// <summary>Logs a trace step.</summary>
        /// <param name="message">The message to trace.</param>
        public static void Info(Format message) => Unit.Instance.LogValue(message);

        /// <summary>Logs a message using custom error type.</summary>
        /// <param name="message">The message to trace.</param>
        public static void Error(Format message) => Unit.Instance.LogErrorValue(message);
    }
}
