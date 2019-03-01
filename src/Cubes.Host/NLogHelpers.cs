using Cubes.Core.Environment;
using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Filters;
using NLog.Targets;
using System;
using System.IO;
using System.Text;


namespace Cubes.Host
{
    public class NLogHelpers
    {
        public static void PrepareNLog(string rootPath)
        {
            const string CONFIG_FILE = "NLog.config";

            LoggingConfiguration config = null;
            try
            {
                var configFile = Path.Combine(rootPath, nameof(CubesFolderKind.Settings), CONFIG_FILE);
                if (File.Exists(configFile))
                    config = new XmlLoggingConfiguration(configFile, false);
            }
            catch (Exception x) { Console.Write(x.Message); }
            finally
            { if (config == null) config = new LoggingConfiguration(); }
            InternalLogger.LogFile = Path.Combine(rootPath, nameof(CubesFolderKind.Logs), "nlog-internal.log");

            if (config.FindTargetByName("console") == null)
            {
                // Swallow Microsoft debug messages
                config.LoggingRules.Add(new LoggingRule("Microsoft.AspNetCore.*", LogLevel.Trace, LogLevel.Warn, new NullTarget()) { Final = true });

                var consoleTarget = new ColoredConsoleTarget("console");
                consoleTarget.Layout = @"${time} | ${level:uppercase=true} | ${logger} :: ${message} ${onexception:${newline}${exception:format=Type,Message,Method,Data,StackTrace:maxInnerExceptionLevel=8:separator=\\r\\n:innerFormat=Type,Message,Method,StackTrace:innerExceptionSeparator=\\r\\n----  Inner Exception  ----\\r\\n}}";
                config.AddTarget("console", consoleTarget);

                var loggingRule = new LoggingRule("*", NLog.LogLevel.Debug, consoleTarget);
                loggingRule.Filters.Add(new WhenContainsFilter { Layout = consoleTarget.Layout, Substring = "Batch acquisition of", Action = FilterResult.Ignore });
                config.LoggingRules.Add(loggingRule);

                // Highlight important strings
                consoleTarget.WordHighlightingRules.Add(
                    new ConsoleWordHighlightingRule
                    {
                        Regex = @"Cubes, version (\d|\.|-|[a-f])+",
                        ForegroundColor = ConsoleOutputColor.Green
                    });
                consoleTarget.WordHighlightingRules.Add(
                    new ConsoleWordHighlightingRule
                    {
                        Regex = @"Cubes listening at (.)+",
                        ForegroundColor = ConsoleOutputColor.Green
                    });
            }

            if (config.FindTargetByName("file") == null)
            {
                var fileTarget = new FileTarget("file")
                {
                    Layout = @"${date}|${level:uppercase=true}|${logger}|${message} ${onexception:${newline}${exception:format=Type,Message,Method,StackTrace:separator=\\r\\n:innerFormat=Type,Message,Method,StackTrace:maxInnerExceptionLevel=8:innerExceptionSeparator=\\r\\n----  Inner Exception  ----\\r\\n}${newline}---- Exception as JSON ----${newline}${exception:format=@}${newline}------- End of JSON -------${newline}}",
                    FileName = "${basedir}/" + nameof(CubesFolderKind.Logs) + "/logfile.txt",
                    ArchiveFileName = "${basedir}/" + nameof(CubesFolderKind.Logs) + "/logfile.{#}.txt",
                    ArchiveNumbering = ArchiveNumberingMode.Date,
                    ArchiveDateFormat = "yyyyMMdd",
                    ArchiveEvery = FileArchivePeriod.Day,
                    MaxArchiveFiles = 7,
                    Encoding = Encoding.UTF8,
                    LineEnding = LineEndingMode.CRLF
                };
                config.AddTarget("file", fileTarget);
                config.LoggingRules.Add(new LoggingRule("*", NLog.LogLevel.Info, fileTarget));
            }

            LogManager.Configuration = config;
            LogManager.ThrowExceptions = true;
            LogManager.ThrowConfigExceptions = true;
        }
    }
}
