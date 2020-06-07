# ConfigSection encryption
Encryp or Decrypt a section in any web.config or app.config file


```
.NET Config Section encription 2.0.0.0

USAGE:
Encrypt multiple sections:
  ConfigSectionCrypt --encrypt --include c:\Path\To\Custom\DLL\SharedFolders.dll --sections configurationStrings,SharedFolders MyApp.exe.config
Decript connectionStrings section:
  ConfigSectionCrypt --dncrypt --sections configurationStrings MyApp.exe.config

  -e, --encrypt           Required. Encrypt the enumerated sections
  -d, --dncrypt           Required. Decrypt the enumerated sections
  -s, --sections          Required. Section names to encrypt / decrypt
  -i, --include           Include reference to neccessary assemblies to read
                          config file
  -l, --loglevel          (Default: Normal) Logging level: Quiet, Normal,
                          Verbose
  --help                  Display this help screen.
  --version               Display version information.
  config_file (pos. 0)    Required. app.config or web.config to encrypt or
                          decrypt
```


**aspnet_regiis** utility is provided as part of .NET framework, but does not provide an easy way to protect or unprotect sections in a desktop .config file.

**ConfigSectionCrypt** is an easy command line tool to encrypt or decryp any section in any .config file
