using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using Serilog;

namespace ConfigSectionCrypt {
    public static class Crypt {

        private static IEnumerable<string> Includes { get; set; }

        public static int EncryptSections(Options options) {
            string fileName = System.IO.Path.GetFileName(options.ConfigFile);
            RegisterAssemblies(options.Include);

            try {
                Configuration config = OpenConfiguration(options.ConfigFile);

                foreach (string sectionName in options.Section) {
                    Log.Debug("Encrypting section '{sectionName}' in '{sectionName}'", sectionName, fileName);
                    ConfigurationSection configSection = config.GetSection(sectionName);
                    if (configSection != null) {
                        if (configSection.SectionInformation.IsProtected) {
                            Log.Warning("'{sectionName}' section is already encrypted", sectionName);
                        } else {
                            configSection.SectionInformation.ProtectSection("DataProtectionConfigurationProvider");
                        }
                        Log.Information("Successfully encrypted section '{sectionName}' in '{fileName}'", sectionName, fileName);
                    } else {
                        Log.Error("ERROR: cannot load the configuration section '{fileName}' from file '{sectionName}'. Remember that section names are case sensitive.", fileName, sectionName);
                        return ExitCodes.SectionNotFound;
                    }
                }
                config.Save();
                return ExitCodes.NoError;
            } catch (Exception ex) {
                string errorType = ex.GetType().FullName;
                Log.Error(ex, "ERROR: {errorType}", errorType);
                return ExitCodes.UnexpectedError;
            }
        }

        public static int DecryptSections(Options options) {
            string fileName = System.IO.Path.GetFileName(options.ConfigFile);
            RegisterAssemblies(options.Include);

            try {
                Configuration config = OpenConfiguration(options.ConfigFile);
                foreach (string sectionName in options.Section) {
                    Log.Debug("Decrypting section '{sectionName}' in '{fileName}'", sectionName, fileName);
                    ConfigurationSection configSection = config.GetSection(sectionName);
                    if (configSection != null) {
                        if (configSection.SectionInformation.IsProtected) {
                            configSection.SectionInformation.UnprotectSection();
                        } else {
                            Log.Warning("'{sectionName}' section is already unprotected", sectionName);
                        }
                        Log.Information("Successfully decrypted section '{sectionName}' in '{fileName}'", sectionName, fileName);
                    } else {
                        Log.Error("ERROR: cannot load the configuration section '{fileName}' from file '{sectionName}'. Remember that section names are case sensitive.", fileName, sectionName);
                        return ExitCodes.SectionNotFound;
                    }
                }
                config.Save();
                return ExitCodes.NoError;
            } catch (Exception ex) {
                string errorType = ex.GetType().FullName;
                Log.Error(ex, "ERROR: {errorType}", errorType);
                return ExitCodes.UnexpectedError;
            }
        }

        private static void RegisterAssemblies(IEnumerable<string> includes) {
            Includes = includes;

            //https://docs.microsoft.com/en-us/dotnet/api/system.appdomain.assemblyresolve
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) {
            Log.Information($"Resolving assembly {args.Name}");
            foreach (string include in Includes) {
                string assemblyName = Path.GetFileNameWithoutExtension(include);
                string assemblyPath = Path.GetFullPath(include);            //Assembly.LoadFile requires absolute path

                if (string.Equals(args.Name, assemblyName, StringComparison.OrdinalIgnoreCase)) {
                    Log.Debug($"Resolved: Loading '{assemblyPath}'.");
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
