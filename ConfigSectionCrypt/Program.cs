using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Reflection;
using System.Web.Configuration;
using CommandLine;
using CommandLine.Text;
using Serilog;
using Serilog.Sinks.SystemConsole;

namespace ConfigSectionCrypt {
    class Program {
        static int Main(string[] args) {

            var parser = new Parser(settings => settings.HelpWriter = null);

            var parsed = parser.ParseArguments<Options>(args);
            return parsed.MapResult(opt => RunAndReturnExitCode(opt), 
                                    err => DisplayHelp(parsed, err));

        }

        private static void SetupLogger(Verbosity verbosity) {
            var logLevel = (Serilog.Events.LogEventLevel)(int)verbosity;
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(logLevel, theme: Serilog.Sinks.SystemConsole.Themes.SystemConsoleTheme.Literate)
                .CreateLogger();
        }

        private static int RunAndReturnExitCode(Options options) {

            SetupLogger(options.LogLevel);

            if (Log.IsEnabled(Serilog.Events.LogEventLevel.Debug)) {
                StringBuilder sb = new StringBuilder();
                string command = options.Operation == 'e' ? "Encrypt" : "Decript";
                sb.AppendLine($"{command} {options.ConfigFile} file");
                sb.AppendLine($"  sections: {string.Join(",", options.Section)}");
                sb.AppendLine($"  includes: {string.Join(",", options.Include)}");
                sb.AppendLine();
                Log.Debug(sb.ToString());
            }

            if (options.Encrypt) {
                return Crypt.EncryptSections(options);
            } else {
                return Crypt.DecryptSections(options);
            }
        }

        private static int DisplayHelp<T>(ParserResult<T> result, IEnumerable<Error> errors) {
            if (errors.IsVersion()) {
                Console.WriteLine(Assembly.GetExecutingAssembly().GetName().Version);
                return 0;
            }
            var helpText = HelpText.AutoBuild(result, h => {
                h.AdditionalNewLineAfterOption = false;
                h.Heading = GetAssemblyDescription();
                h.Copyright = " ";
                return HelpText.DefaultParsingErrorsHandler(result, h);
            }, e => e);
            Console.WriteLine(helpText);
            return ExitCodes.UnexpectedError;
        }

        private static string GetAssemblyDescription() {
            var assembly = Assembly.GetExecutingAssembly();
            var description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>().Description ?? assembly.GetName().Name;
            return $"{description} {assembly.GetName().Version}";
        }

    }
}
