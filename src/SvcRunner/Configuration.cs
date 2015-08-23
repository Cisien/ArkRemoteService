using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SvcRunner
{
    internal static class Configuration
    {
        internal static string ArkRemotePath { get; } = ConfigurationManager.AppSettings[nameof(ArkRemotePath)];

        internal static string LogFilePath { get; } = ConfigurationManager.AppSettings[nameof(LogFilePath)];

        internal static bool LogOutputToFile { get; } = bool.Parse(ConfigurationManager.AppSettings[nameof(LogOutputToFile)]);

        internal static float MinutesForShutdown { get; } = float.Parse(ConfigurationManager.AppSettings[nameof(MinutesForShutdown)]);
    }
}
