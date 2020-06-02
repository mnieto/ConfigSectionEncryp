using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace ConfigSectionCrypt {
    public static class Crypt {

        public static int EncryptSections(Options options) {
            string fileName = System.IO.Path.GetFileName(options.ConfigFile);

            try {
                Configuration config = OpenConfiguration(options.ConfigFile);
                LoadAssemblies(options.Include);

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

            try {
                LoadAssemblies(options.Include);
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

        private static void LoadAssemblies(IEnumerable<string> include) {
            //TODO: Load assemblies from the Include option
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
