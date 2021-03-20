using System;

namespace FeatureUsage
{
    public static class Log
    {
        static void _write(string category, string message, params object[] args)
        {
            if(args.Length > 0)
                System.Diagnostics.Debug.WriteLine($"{category}: {message}", args);
            else
                System.Diagnostics.Debug.WriteLine($"{category}: {message}");
        }

        public static void Debug(string message, params object[] args)
            => _write("DEBUG", message, args);

        public static void Information(string message, params object[] args)
            => _write("INFO", message, args);

        public static void Error(string message, params object[] args)
            => _write("ERROR", message, args);
        public static void Error(string message, Exception ex, params object[] args)
            => _write("ERROR", message, args);
        public static void Error(Exception ex)
            => _write("ERROR", "{0}", ex);

        public static void Warning(string message, params object[] args)
            => _write("WARN", message, args);
        public static void Warning(string message, Exception ex, params object[] args)
            => _write("WARN", message, args);
        public static void Warning(Exception ex)
            => _write("WARN", "{0}", ex);
    }
}
