using System.Collections.Generic;
using CommandLine;

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

    internal char Operation { get; set; }
}
