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

    /// <summary>Additional extension methods to potentially include in the ContainerExpressions library.</summary>
    public static class ContainerExtensions
    {
        // Should "response.Transform(_ => Unit.Instance)" support T value: "response.Transform(Unit.Instance)" as well?

        /// <summary>
        /// Executes one of the functions when the input response is valid, otherwise an invalid response is returned.
        /// <para>When the condition is true the first function is executed, otherwise the second function is executed.</para>
        /// </summary>
        public static Response<TResult> Pivot<T, TResult>(this Response<T> response, bool condition, TResult result1, TResult result2) => response ? (condition ? Response.Create(result1) : Response.Create(result2)) : new Response<TResult>();

        /// <summary>
        /// Executes one of the functions when the input response is valid, otherwise an invalid response is returned.
        /// <para>When the condition is true the first function is executed, otherwise the second function is executed.</para>
        /// </summary>
        public static Response<TResult> Pivot<T, TResult>(this Response<T> response, Func<T, bool> condition, TResult result1, TResult result2) => response ? (condition(response) ? Response.Create(result1) : Response.Create(result2)) : new Response<TResult>();
    }
}
