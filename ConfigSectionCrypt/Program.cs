using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Reflection;
using System.Web.Configuration;

namespace ConfigSectionCrypt {
    class Program {
        static void Main(string[] args) {

            ShowTitle();

            if (args.Length < 3) {
                ShowUsage();
                return;
            }

            string operation = args[0].ToLower();
            string configSectionName = args[1];
            string configFileName = args[2];

            if (operation == "-e" || operation == "/e" ||
                operation == "-enc" || operation == "/enc" ||
                operation == "-encrypt" || operation == "/encrypt") {
                    EncryptSection(configFileName, configSectionName);
            } else if (operation == "-d" || operation == "/d" ||
                      operation == "-dec" || operation == "/dec" ||
                      operation == "-decrypt" || operation == "/decrypt") {
                DecryptSection(configFileName, configSectionName);
            } else {
                Console.WriteLine("ERROR: unknown operation ({0}) specified", operation);
                ShowUsage();
                Environment.ExitCode = ExitCodes.ParametersError;
            }
        }


        private static void ShowUsage() {
            Console.WriteLine("USAGE:  ConfigSectionCrypt (-e | -d) section filename");
            Console.WriteLine("           -e/-encrypt    Encrypt the specified section in the given file");
            Console.WriteLine("           -d/-decrypt    Decrypt the specified section in the given file");
            Console.WriteLine();
            Console.WriteLine("Exit codes:");
            Console.WriteLine("           0:  Executed without errors");
            Console.WriteLine("           1:  Cannot open section or section does not exists");
            Console.WriteLine("           2:  Unexpected error");
        }

        private static void ShowTitle() {
            Console.WriteLine(".NET Config Section encription, v{0}", Assembly.GetExecutingAssembly().GetName().Version);
            Console.WriteLine();
        }

        private static void DecryptSection(string configFileName, string sectionName) {
            string fileName = System.IO.Path.GetFileName(configFileName);
            Console.WriteLine("Decrypting section '{0}' in '{1}'", sectionName, fileName);

            try {
                Configuration config = OpenConfiguration(configFileName);

                ConfigurationSection configSection = config.GetSection(sectionName);
                if (configSection != null) {
                    if (configSection.SectionInformation.IsProtected) {
                        configSection.SectionInformation.UnprotectSection();
                        config.Save();
                    } else {
                        Console.WriteLine("'{0}' section is already unprotected", sectionName);
                    }
                    Console.WriteLine("Successfully decrypted section '{0}' in '{1}'", sectionName, fileName);
                } else {
                    Console.WriteLine("ERROR: cannot load the configuration section '{0}' from file '{1}' - section not found or invalid", fileName, sectionName);
                    Environment.ExitCode = ExitCodes.SectionNotFound;
                }
            } catch (Exception ex) {
                Console.WriteLine("ERROR: {0}: {1}", ex.GetType().FullName, ex.Message);
                Environment.ExitCode = ExitCodes.UnexpectedError;
            }
        }

        private static void EncryptSection(string configFileName, string sectionName) {
            string fileName = System.IO.Path.GetFileName(configFileName);
            Console.WriteLine("Encrypting section '{0}' in '{1}'", sectionName, fileName);

            try {
                Configuration config = OpenConfiguration(configFileName);

                ConfigurationSection configSection = config.GetSection(sectionName);
                if (configSection != null) {
                    if (configSection.SectionInformation.IsProtected) {
                        Console.WriteLine("'{0}' section is already encrypted", sectionName);
                    } else {
                        configSection.SectionInformation.ProtectSection("DataProtectionConfigurationProvider");
                        config.Save();
                    }
                    Console.WriteLine("Successfully encrypted section '{0}' in '{1}'", sectionName, fileName);
                } else {
                    Console.WriteLine("ERROR: cannot load the configuration section '{0}' from file '{1}'", fileName, sectionName);
                    Environment.ExitCode = ExitCodes.SectionNotFound;
                }
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
