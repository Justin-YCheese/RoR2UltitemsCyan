using BepInEx.Logging;

namespace UltitemsCyan
{
    internal static class Log
    {
        private static ManualLogSource _logSource;


        // Make a setting mostly for myself or other debugging
        private const bool EnableDebugLogs = true;

        internal static void Init(ManualLogSource logSource)
        {
            _logSource = logSource;
        }

        internal static void Debug(object data) => _logSource.LogDebug(data);
        internal static void Error(object data) => _logSource.LogError(data);
        internal static void Fatal(object data) => _logSource.LogFatal(data);
        internal static void Info(object data) => _logSource.LogInfo(data);
        internal static void Message(object data) => _logSource.LogMessage(data);
        internal static void Warning(object data) => _logSource.LogWarning(data);

        internal static void Logger(LogLevel level, string log)
        {
            if (!EnableDebugLogs)
            {
#pragma warning disable CS0162 // Unreachable code detected
                return;
#pragma warning restore CS0162 // Unreachable code detected
            }
            switch (level)
            {
                case LogLevel.Info:
                    Info(log);
                    break;
                case LogLevel.Debug:
                    Debug(log);
                    break;
                case LogLevel.Warning:
                    Warning(log);
                    break;
                case LogLevel.Error:
                    Error(log);
                    break;
                case LogLevel.Fatal:
                    Fatal(log);
                    break;
                default:
                    Debug(log);
                    break;
            }
        }
    }
}