using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace ConfigSectionCrypt {
    public static class Crypt {

        private static IEnumerable<string> Includes { get; set; }

        public static int EncryptSections(Options options) {
            string fileName = System.IO.Path.GetFileName(options.ConfigFile);
            RegisterAssemblies(options.Include);

            try {
                Configuration config = OpenConfiguration(options.ConfigFile);

                foreach (string sectionName in options.Section) {
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
                        return ExitCodes.SectionNotFound;
                    }
                }
                config.Save();
                return ExitCodes.NoError;
            } catch (Exception ex) {
                Console.WriteLine("ERROR: {0}: {1}", ex.GetType().FullName, ex.Message);
                return ExitCodes.UnexpectedError;
            }
        }

        public static int DecryptSections(Options options) {
            string fileName = System.IO.Path.GetFileName(options.ConfigFile);
            RegisterAssemblies(options.Include);

            try {
                Configuration config = OpenConfiguration(options.ConfigFile);
                foreach (string sectionName in options.Section) {
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
                        return ExitCodes.SectionNotFound;
                    }
                }
                config.Save();
                return ExitCodes.NoError;
            } catch (Exception ex) {
                Console.WriteLine("ERROR: {0}: {1}", ex.GetType().FullName, ex.Message);
                return ExitCodes.UnexpectedError;
            }
        }

        private static void RegisterAssemblies(IEnumerable<string> includes) {
            Includes = includes;

            //https://docs.microsoft.com/en-us/dotnet/api/system.appdomain.assemblyresolve
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) {
            Console.WriteLine($"Resolving assembly {args.Name}");
            foreach (string include in Includes) {
                string assemblyName = Path.GetFileNameWithoutExtension(include);
                string assemblyPath = Path.GetFullPath(include);            //Assembly.LoadFile requires absolute path

                if (string.Equals(args.Name, assemblyName, StringComparison.OrdinalIgnoreCase)) {
                    Console.WriteLine($"Resolved: Loading '{assemblyPath}'.");
                    return Assembly.LoadFile(assemblyPath);
                }
            }
            throw new FileNotFoundException($"Cannot find a reference for the assembly '{args.Name}'.");
        }

        private static Configuration OpenConfiguration(string filename) {
            if (filename.Trim().ToLower().EndsWith("web.config")) {
                return WebConfigurationManager.OpenWebConfiguration(filename);
            } else {
                var map = new ExeConfigurationFileMap() {
                    ExeConfigFilename = filename
                };
                return ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            }
        }

    }
}
