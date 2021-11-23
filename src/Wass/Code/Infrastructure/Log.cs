namespace Wass.Code.Infrastructure
{
    public static class Log
    {
        private static Action<string> _traceLogger = x => { };
        private static Action<string> _errorLogger = x => { };

        /// <summary>
        /// Set your desired logging implementation here.
        /// <para>It is recommend that the logger be stateless.</para>
        /// </summary>
        /// <param name="traceLogger">A function that will log the incoming trace message.</param>
        /// <param name="errorLogger">A function that will log the incoming error message.</param>
        public static void SetLogger(Action<string> traceLogger, Action<string> errorLogger)
        {
            if (traceLogger == null) throw new ArgumentNullException(nameof(traceLogger));
            if (errorLogger == null) throw new ArgumentNullException(nameof(errorLogger));

            _traceLogger = traceLogger;
            _errorLogger = errorLogger;
        }

        internal static void Trace(string message) => _traceLogger(message);

        internal static void Error(string message) => _errorLogger(message);

        internal static void Error(Exception ex) => Error(ex, ex.Message);

        internal static void Error(Exception ex, string message)
        {
            if (!string.IsNullOrEmpty(message)) Error(message);
            if (ex == null) return;
            Error(ex.ToString());
            if (ex is AggregateException ae)
            {
                foreach (var e in ae.Flatten().InnerExceptions)
                {
                    Error(e.ToString());
                }
            }
        }
    }

    internal static class LogExtensions
    {
        public static T Trail<T>(this T value, string message)
        {
            Log.Trace(message);
            return value;
        }

        public static T Trail<T>(this T value, Func<T, string> format)
        {
            Log.Trace(format(value));
            return value;
        }
    }
}
