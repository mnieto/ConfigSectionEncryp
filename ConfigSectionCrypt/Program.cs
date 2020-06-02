using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Reflection;
using System.Web.Configuration;
using CommandLine;
using CommandLine.Text;

namespace ConfigSectionCrypt {
    class Program {
        static void Main(string[] args) {

            var parser = new Parser(settings => settings.HelpWriter = null);

            var parsed = parser.ParseArguments<Options>(args);
            parsed.MapResult(opt => RunAndReturnExitCode(opt), 
                             err => DisplayHelp(parsed, err));

        }


        private static int RunAndReturnExitCode(Options options) {
            string command = options.Operation == 'e' ? "Encrypt" : "Decript";
            Console.WriteLine($"{command} {options.ConfigFile} file");
            Console.WriteLine($"  sections: {string.Join(",", options.Section)}");
            Console.WriteLine($"  includes: {string.Join(",", options.Include)}");
            Console.WriteLine();

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
