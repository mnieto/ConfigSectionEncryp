using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

public class Options {
    [Option('e', "encrypt", SetName = "encrypt", HelpText = "Encrypt the enumerated sections", Required = true)]
    public bool Encrypt { get => Operation == 'e'; set => Operation = value ? 'e' : 'd'; }

    [Option('d', "dncrypt", SetName = "decrypt", HelpText = "Decrypt the enumerated sections", Required = true)]
    public bool Decrypt { get => Operation == 'd'; set => Operation = value ? 'd' : 'e'; }

    [Value(0, MetaName = "config_file", HelpText = "app.config or web.config to encrypt or decrypt", Required = true)]
    public string ConfigFile { get; set; }

    [Option('s', "sections", HelpText = "Section names to encrypt / decrypt", Required = true, Separator = ',')]
    public IEnumerable<string> Section { get; set; }

    [Option('i', "include", HelpText = "Include reference to neccessary assemblies to read config file", Required = false, Separator = ',')]
    public IEnumerable<string> Include { get; set; }

    [Option('l', "loglevel", HelpText = "Logging level: Quiet, Normal, Verbose", Default = Verbosity.Normal)]
    public Verbosity LogLevel { get; set; }

    [Usage()]
    public static IEnumerable<Example> Usage {
        get {
            yield return new Example("Encrypt multiple sections", new Options {
                Encrypt = true,
                ConfigFile = "MyApp.exe.config",
                Section = "configurationStrings,SharedFolders".Split(','),
                Include = @"c:\Path\To\Custom\DLL\SharedFolders.dll".Split(',')
            });
            yield return new Example("Decript connectionStrings section", new Options {
                Decrypt = true,
                ConfigFile = "MyApp.exe.config",
                Section = "configurationStrings".Split(',')
            });
        }
    }

    internal char Operation { get; set; }
}
