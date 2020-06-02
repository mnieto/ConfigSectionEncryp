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
            //TODO: Load assemblies from the Include option
            if (options.Encrypt) {
                EncryptSection(options.ConfigFile, options.Section);
            } else {
                DecryptSection(options.ConfigFile, options.Section);
            }

            return Environment.ExitCode;
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


        private static void DecryptSection(string configFileName, IEnumerable<string> sections) {
            string fileName = System.IO.Path.GetFileName(configFileName);
            try {
                Configuration config = OpenConfiguration(configFileName);
                foreach(string sectionName in sections) {
                    Console.WriteLine("Decrypting section '{0}' in '{1}'", sectionName, fileName);
                    ConfigurationSection configSection = config.GetSection(sectionName);
                    if (configSection != null) {
                        if (configSection.SectionInformation.IsProtected) {
                            configSection.SectionInformation.UnprotectSection();
                        } else {
                            Console.WriteLine("'{0}' section is already unprotected", sectionName);
                        }
                        Console.WriteLine("Successfully decrypted section '{0}' in '{1}'", sectionName, fileName);
                    } else {
                        Console.WriteLine("ERROR: cannot load the configuration section '{0}' from file '{1}' - section not found or invalid", fileName, sectionName);
                        Environment.ExitCode = ExitCodes.SectionNotFound;
                    }
                }
                config.Save();
            } catch (Exception ex) {
                Console.WriteLine("ERROR: {0}: {1}", ex.GetType().FullName, ex.Message);
                Environment.ExitCode = ExitCodes.UnexpectedError;
            }
        }

        private static void EncryptSection(string configFileName, IEnumerable<string> sections) {
            string fileName = System.IO.Path.GetFileName(configFileName);
            try {
                Configuration config = OpenConfiguration(configFileName);

                foreach (string sectionName in sections) {
                    Console.WriteLine("Encrypting section '{0}' in '{1}'", sectionName, fileName);
                    ConfigurationSection configSection = config.GetSection(sectionName);
                    if (configSection != null) {
                        if (configSection.SectionInformation.IsProtected) {
                            Console.WriteLine("'{0}' section is already encrypted", sectionName);
                        } else {
                            configSection.SectionInformation.ProtectSection("DataProtectionConfigurationProvider");
                        }
                        Console.WriteLine("Successfully encrypted section '{0}' in '{1}'", sectionName, fileName);
                    } else {
                        Console.WriteLine("ERROR: cannot load the configuration section '{0}' from file '{1}'", fileName, sectionName);
                        Environment.ExitCode = ExitCodes.SectionNotFound;
                    }
                }
                config.Save();
            } catch (Exception ex) {
                Console.WriteLine("ERROR: {0}: {1}", ex.GetType().FullName, ex.Message);
                Environment.ExitCode = ExitCodes.UnexpectedError;
            }
        }

        private static Configuration OpenConfiguration(string filename) {
            if (filename.Trim().ToLower().EndsWith("web.config")) {
                return WebConfigurationManager.OpenWebConfiguration(filename);
            } else {
                ExeConfigurationFileMap map = new ExeConfigurationFileMap() { 
                    ExeConfigFilename = filename
                };
                return ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            }
        }

        private class ExitCodes {
            public const int ParametersError = 1;
            public const int SectionNotFound = 2;
            public const int UnexpectedError = 3;
        }
    }
}
